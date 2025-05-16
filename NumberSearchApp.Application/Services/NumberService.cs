using NumberSearchApp.Domain.Entities;
using NumberSearchApp.Domain.Repositories;
using NumberSearchApp.Domain.Services;

namespace NumberSearchApp.Application.Services;

public class NumberService : INumberService
{
    private readonly INumberRepository _numberRepository;
    private readonly ISortingService _sortingService;

    public NumberService(
        INumberRepository numberRepository,
        ISortingService sortingService)
    {
        _numberRepository = numberRepository;
        _sortingService = sortingService;
    }

    public async Task<int[]> GenerateAndSortNumbersAsync()
    {
        var numbers = await _numberRepository.GetRandomNumbersAsync();
        _sortingService.QuickSort(numbers, 0, numbers.Length - 1);
        return numbers;
    }

    public int BinarySearch(int[] numbers, int value)
    {
        return _sortingService.BinarySearch(numbers, value);
    }

    public int BinarySearch(int[] numbers, int value, out int iterations)
    {
        return _sortingService.BinarySearch(numbers, value, out iterations);
    }

    public async Task<SearchResult> SearchNumberAsync(int value)
    {
        using var activity = System.Diagnostics.Activity.Current?.Source?.StartActivity(
            "SearchNumber",
            System.Diagnostics.ActivityKind.Server);

        activity?.SetTag("search_value", value);

        try
        {
            var numbers = await _numberRepository.GetRandomNumbersAsync();
            activity?.SetTag("array_size", numbers.Length);

            _sortingService.QuickSort(numbers, 0, numbers.Length - 1);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int index = _sortingService.BinarySearch(numbers, value);
            stopwatch.Stop();

            activity?.SetTag("search_time_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetTag("result_found", index != -1);

            return new SearchResult
            {
                Found = index != -1,
                Index = index,
                Value = value
            };
        }
        catch (Exception ex)
        {
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error);
            activity?.AddException(ex);
            throw;
        }
    }
}