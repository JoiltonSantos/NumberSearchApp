using NumberSearchApp.Domain.Services;

namespace NumberSearchApp.Infrastructure.Services;

public class SortingService : ISortingService
{
    public void QuickSort(int[] array, int left, int right)
    {
        if (left < right)
        {
            int pivotIndex = Partition(array, left, right);
            QuickSort(array, left, pivotIndex - 1);
            QuickSort(array, pivotIndex + 1, right);
        }
    }

    private int Partition(int[] array, int left, int right)
    {
        int pivot = array[right];
        int i = left - 1;

        for (int j = left; j < right; j++)
        {
            if (array[j] <= pivot)
            {
                i++;
                Swap(array, i, j);
            }
        }

        Swap(array, i + 1, right);
        return i + 1;
    }

    private void Swap(int[] array, int i, int j)
    {
        int temp = array[i];
        array[i] = array[j];
        array[j] = temp;
    }

    public int BinarySearch(int[] array, int value)
    {
        int iterations;
        return BinarySearch(array, value, out iterations);
    }

    public int BinarySearch(int[] array, int value, out int iterations)
    {
        int left = 0;
        int right = array.Length - 1;
        iterations = 0;

        while (left <= right)
        {
            iterations++;
            int mid = left + (right - left) / 2;

            if (array[mid] == value)
                return mid;

            if (array[mid] < value)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return -1;
    }
}