#nullable enable
using AutoMapper;
using ImoutoRebirth.RoomService.WebApi.Client;

namespace ImoutoRebirth.Navigator.Services.Collections;

internal class DestinationFolderService : IDestinationFolderService
{
    private readonly DestinationFolderClient _destinationFolderClient;
    private readonly IMapper _mapper;

    public DestinationFolderService(IMapper mapper, DestinationFolderClient destinationFolderClient)
    {
        _mapper = mapper;
        _destinationFolderClient = destinationFolderClient;
    }

    public async Task<DestinationFolder?> GetDestinationFolderAsync(Guid collectionId)
    {
        try
        {
            var result = await _destinationFolderClient.GetAsync(collectionId);
            return _mapper.Map<DestinationFolder>(result);
        }
        catch (WebApiException e) when (e.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<DestinationFolder> AddOrUpdateDestinationFolderAsync(DestinationFolder destinationFolder)
    {
        var request = _mapper.Map<DestinationFolderCreateRequest>(destinationFolder);

        var result = await _destinationFolderClient.CreateOrUpdateAsync(
            destinationFolder.CollectionId,
            request);

        return _mapper.Map<DestinationFolder>(result);
    }

    public Task DeleteDestinationFolderAsync(Guid collectionId, Guid destinationFolderId)
        => _destinationFolderClient.DeleteAsync(collectionId, destinationFolderId);
}
