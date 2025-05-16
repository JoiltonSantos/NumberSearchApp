namespace NumberSearchApp.Domain.Services;

public interface ISortingService
{
    void QuickSort(int[] array, int left, int right);
    int BinarySearch(int[] array, int value);
    int BinarySearch(int[] array, int value, out int iterations);
}