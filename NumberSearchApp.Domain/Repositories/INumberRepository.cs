namespace NumberSearchApp.Domain.Repositories;

public interface INumberRepository
{
    Task<int[]> GetRandomNumbersAsync();
}