using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Lilin.WebApi.Client.Models;
using ImoutoRebirth.Room.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client.Models;

namespace ImoutoRebirth.Navigator.Services.Tags
{
    class FileService : IFileService
    {
        private readonly IImoutoRebirthLilinWebApiClient _lilinClient;
        private readonly IMapper _mapper;
        private readonly IImoutoRebirthRoomWebApiClient _roomClient;

        public FileService(
            IImoutoRebirthLilinWebApiClient lilinClient,
            IImoutoRebirthRoomWebApiClient roomClient,
            IMapper mapper)
        {
            _lilinClient = lilinClient;
            _roomClient = roomClient;
            _mapper = mapper;
        }

        public async Task<IReadOnlyCollection<File>> SearchFiles(
            Guid? collectionId, 
            IReadOnlyCollection<SearchTag> tags, 
            int take, 
            int skip)
        {
            var tagsSearch = await _lilinClient.Files
                .GetFilesByTagsAsync(
                    new FilesSearchRequest(
                        _mapper.Map<List<TagSearchEntryRequest>>(tags), take, skip));

            var filesSearch = await _roomClient.CollectionFiles
                .SearchAsync(new CollectionFilesRequest(collectionId, tagsSearch));

            return _mapper.Map<IReadOnlyCollection<File>>(filesSearch);
        }

        public async Task<int> CountFiles(Guid? collectionId, IReadOnlyCollection<SearchTag> tags)
        {
            return (await _lilinClient.Files
                .GetFilesCountByTagsAsync(
                    new FilesSearchRequest(
                        _mapper.Map<List<TagSearchEntryRequest>>(tags)))).Value;
        }

        public Task RemoveFile(Guid fileId)
        {
            throw new NotImplementedException();
        }
    }

    public class OrganizationProfile : Profile
    {
        public OrganizationProfile()
        {
            CreateMap<SearchTag, TagSearchEntryRequest>()
                .ForMember(x => x.TagId, o => o.MapFrom(x => x.Tag.Id.Value))
                .ForMember(x => x.Value, o => o.MapFrom(x => x.Value))
                .ForMember(x => x.TagSearchScope, o => o.MapFrom(x => x.SearchType));

            CreateMap<SearchType, TagSearchScope>()
                .ConvertUsing(
                    (x, _) => x switch
                    {
                        SearchType.Include => TagSearchScope.Included,
                        SearchType.Exclude => TagSearchScope.Excluded,
                        _ => throw new NotImplementedException(x.ToString())
                    });

            CreateMap<CollectionFileResponse, File>();
        }
    }
}