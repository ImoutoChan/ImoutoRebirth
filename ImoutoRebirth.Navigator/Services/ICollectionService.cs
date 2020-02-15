using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImoutoRebirth.Navigator.Services
{
    public interface ICollectionService
    {
        Task<IReadOnlyCollection<Collection>> GetAllCollectionsAsync();

        Task<Collection> CreateCollectionAsync(string name);

        Task<Collection> RenameCollection(string name);

        Task DeleteCollectionAsync(Guid guid);
    }
}