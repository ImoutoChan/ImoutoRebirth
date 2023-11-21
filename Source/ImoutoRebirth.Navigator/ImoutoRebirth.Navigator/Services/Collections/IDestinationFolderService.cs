namespace ImoutoRebirth.Navigator.Services.Collections;

public interface IDestinationFolderService
{
    Task<DestinationFolder?> GetDestinationFolderAsync(Guid collectionId);

    Task<DestinationFolder> SetDestinationFolderAsync(DestinationFolder destinationFolder);

    Task DeleteDestinationFolderAsync(Guid collectionId);
}
