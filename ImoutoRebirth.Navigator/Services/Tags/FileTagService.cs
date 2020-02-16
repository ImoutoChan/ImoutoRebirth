using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.Tags
{
    internal class FileTagService : IFileTagService
    {
        private readonly IImoutoRebirthLilinWebApiClient _lilinClient;
        private readonly IMapper _mapper;

        public FileTagService(IImoutoRebirthLilinWebApiClient lilinClient, IMapper mapper)
        {
            _lilinClient = lilinClient;
            _mapper = mapper;
        }

        public Task SetRate(Guid fileId, Rate rate)
        {
            throw new NotImplementedException();
        }

        public Task SetFavorite(Guid fileId, bool value)
        {
            throw new NotImplementedException();
        }

        public Task BindTags(IReadOnlyCollection<FileTag> fileTags)
        {
            throw new NotImplementedException();
        }

        public Task UnbindTag(Guid fileId, Guid tagId)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyCollection<FileTag>> GetFileTags(Guid fileId)
        {
            var info = await _lilinClient.Files.GetFileInfoAsync(fileId);

            return _mapper.Map<IReadOnlyCollection<FileTag>>(info.Tags);
        }
    }
}