using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.Core.Services.Abstract;
using MassTransit;

namespace ImoutoRebirth.Room.Infrastructure.Service
{
    internal class RemoteCommandService : IRemoteCommandService
    {
        private readonly IBus _bus;

        public RemoteCommandService(IBus bus)
        {
            _bus = bus;
        }
        
        public async Task UpdateMetadataRequest(Guid fileId, string md5)
        {
            var command = new
            {
                Md5 = md5,
                FileId = fileId
            };

            await _bus.Send<INewFileCommand>(command);
        }

        public async Task SaveTags(Guid fileId, IReadOnlyCollection<string> tags)
        {
            var command = new
            {
                FileId = fileId,
                MetadataSource = MetadataSource.Manual,
                FileTags = tags.Select(x => new
                {
                    Type = "location",
                    Name = x
                }).ToArray()
            };

            await _bus.Send<IUpdateMetadataCommand>(command);
        }
    }
}