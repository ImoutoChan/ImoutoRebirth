namespace ImoutoRebirth.Room.DataAccess.Cache;

public interface ICollectionFileCacheService
{
    void AddToFilter(Guid id, string value);

    Task<bool> GetResultOrCreateFilterAsync(
        Guid id,
        string value,
        Func<Guid, string, Task<bool>> checkFunc,
        Func<Guid, Task<List<string>>> addFunc);
}