using NumberSearchApp.Domain.Entities;

namespace NumberSearchApp.Domain.Services;

public interface INumberService
{
    Task<SearchResult> SearchNumberAsync(int value);
    Task<int[]> GenerateAndSortNumbersAsync();
    int BinarySearch(int[] numbers, int value);
    int BinarySearch(int[] numbers, int value, out int iterations);
}