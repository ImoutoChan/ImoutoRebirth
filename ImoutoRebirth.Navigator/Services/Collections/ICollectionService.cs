namespace ImoutoRebirth.Navigator.Services.Collections;

public interface ICollectionService
{
    Task<IReadOnlyCollection<Collection>> GetAllCollectionsAsync();

    Task<Collection> CreateCollectionAsync(string name);

    Task RenameCollection(Guid collectionId, string name);

    Task DeleteCollectionAsync(Guid guid);
}