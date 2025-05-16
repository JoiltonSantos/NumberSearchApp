using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NumberSearchApp.Domain.Repositories;
using NumberSearchApp.Domain.Services;
using NumberSearchApp.Infrastructure.Repositories;
using NumberSearchApp.Infrastructure.Services;
using NumberSearchApp.Application.Services;
using System.Diagnostics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Elastic.Apm;
using Elastic.Apm.DiagnosticSource;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("NumberSearchApp")
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("NumberSearchApp"))
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    });

builder.Services.AddHttpClient<INumberRepository, NumberRepository>();
builder.Services.AddTransient<ISortingService, SortingService>();
builder.Services.AddTransient<INumberService, NumberService>();

var host = builder.Build();

Agent.Subscribe(new HttpDiagnosticsSubscriber());

var tracer = new ActivitySource("NumberSearchApp");

using (var activity = tracer.StartActivity("NumberSearchApp"))
{
    var numberService = host.Services.GetRequiredService<INumberService>();
    int[]? numbers = null;

    async Task LoadNumbers()
    {
        Console.WriteLine("\nCarregando novos números...");
        try
        {
            numbers = await numberService.GenerateAndSortNumbersAsync();
            if (numbers != null)
            {
                Console.WriteLine($"\nNúmeros gerados: {numbers.Length}");
                Console.WriteLine($"Menor número: {numbers.Min()}, Maior número: {numbers.Max()}");
                Console.WriteLine("Lista carregada com sucesso!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao gerar números: {ex.Message}");
            activity?.AddException(ex);
            numbers = null;
        }
    }

    await LoadNumbers();

    while (true)
    {
        Console.WriteLine("\n====================================");
        Console.WriteLine("Digite um número para buscar");
        Console.WriteLine("Digite 'novo' para gerar nova lista");
        Console.WriteLine("Digite 'listar' para mostrar todos os números");
        Console.WriteLine("====================================");
        Console.Write("Sua escolha: ");

        var input = Console.ReadLine();

        if (input?.ToLower() == "novo")
        {
            await LoadNumbers();
            continue;
        }

        if (input?.ToLower() == "listar")
        {
            if (numbers == null)
            {
                Console.WriteLine("Erro: nenhuma lista de números foi carregada. Digite 'novo' para carregar uma lista.");
                continue;
            }

            try
            {
                using var listActivity = tracer.StartActivity("ListAllNumbers", ActivityKind.Server);
                listActivity?.SetTag("service.name", "number-search-app");

                Console.WriteLine("\n=== Lista de todos os números ===");

                const int colWidth = 6;
                const int cols = 10;

                for (int i = 0; i < numbers.Length; i++)
                {
                    Console.Write($"{numbers[i],colWidth}");

                    if ((i + 1) % cols == 0 || i == numbers.Length - 1)
                    {
                        Console.WriteLine();
                    }
                }

                Console.WriteLine($"\nTotal de números: {numbers.Length}");
                Console.WriteLine($"Menor número: {numbers.Min()}");
                Console.WriteLine($"Maior número: {numbers.Max()}");

                listActivity?.SetTag("numbers_count", numbers.Length);
            }
            catch (Exception ex)
            {
                activity?.AddException(ex);
                Console.WriteLine($"Erro ao listar números: {ex.Message}");
            }

            continue;
        }

        if (int.TryParse(input, out int searchValue))
        {
            if (numbers == null)
            {
                Console.WriteLine("Erro: nenhuma lista de números foi carregada. Digite 'novo' para carregar uma lista.");
                continue;
            }

            try
            {
                using var searchActivity = tracer.StartActivity("PerformSearch", ActivityKind.Server);
                searchActivity?.SetTag("service.name", "number-search-app");

                Console.WriteLine($"\nBuscando pelo número {searchValue}...");

                int iterations = 0;
                int index = numberService.BinarySearch(numbers, searchValue, out iterations);

                searchActivity?.SetTag("iterations", iterations);
                searchActivity?.SetTag("found", index != -1);

                if (index != -1)
                {
                    Console.WriteLine($"Número {searchValue} encontrado na posição {index}");
                    Console.WriteLine($"A busca binária usou {iterations} iterações");
                }
                else
                {
                    Console.WriteLine($"Número {searchValue} não foi encontrado");
                    Console.WriteLine($"A busca binária usou {iterations} iterações");
                }
            }
            catch (Exception ex)
            {
                activity?.AddException(ex);
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Por favor, digite um número válido, 'novo' ou 'listar'.");
        }
    }
}
