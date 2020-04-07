#nullable enable
using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Room.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client.Models;
using Microsoft.Rest;
using Microsoft.Rest.TransientFaultHandling;

namespace ImoutoRebirth.Navigator.Services.Collections
{
    internal class DestinationFolderService : IDestinationFolderService
    {
        private readonly IDestinationFolder _destinationFolderRoomClient;
        private readonly IMapper _mapper;

        public DestinationFolderService(IDestinationFolder destinationFolderRoomClient, IMapper mapper)
        {
            _destinationFolderRoomClient = destinationFolderRoomClient;
            _mapper = mapper;
        }

        public async Task<DestinationFolder?> GetDestinationFolderAsync(Guid collectionId)
        {
            try
            {
                var result = await _destinationFolderRoomClient.GetAsync(collectionId);
                return _mapper.Map<DestinationFolder>(result);
            }
            catch (HttpOperationException e) when (e.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<DestinationFolder> AddOrUpdateDestinationFolderAsync(DestinationFolder destinationFolder)
        {
            var request = _mapper.Map<DestinationFolderCreateRequest>(destinationFolder);

            var result = await _destinationFolderRoomClient.CreateOrUpdateAsync(
                destinationFolder.CollectionId,
                request);

            return _mapper.Map<DestinationFolder>(result);
        }

        public Task DeleteDestinationFolderAsync(Guid collectionId, Guid destinationFolderId)
            => _destinationFolderRoomClient.DeleteAsync(collectionId, destinationFolderId);
        
    }
}