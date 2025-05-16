using NumberSearchApp.Domain.Repositories;
using System.Net.Http.Json;

namespace NumberSearchApp.Infrastructure.Repositories;

public class NumberRepository : INumberRepository
{
    private readonly HttpClient _httpClient;

    public NumberRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int[]> GetRandomNumbersAsync()
    {
        var response = await _httpClient.GetAsync("http://www.randomnumberapi.com/api/v1.0/random?min=1&max=100&count=95");
        response.EnsureSuccessStatusCode();

        var numbers = await response.Content.ReadFromJsonAsync<int[]>();
        return numbers ?? Array.Empty<int>();
    }
}