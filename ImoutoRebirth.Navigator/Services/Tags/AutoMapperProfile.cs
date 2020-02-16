using System;
using AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client.Models;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Room.WebApi.Client.Models;

namespace ImoutoRebirth.Navigator.Services.Tags
{
    internal class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
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

            CreateFileTagMaps();
        }

        private void CreateFileTagMaps()
        {
            CreateMap<FileTagResponse, FileTag>();

            CreateMap<TagResponseType, TagType>()
                .ForCtorParam("title", o => o.MapFrom(x => x.Name));

            CreateMap<FileTagResponseTag, Tag>()
                .ForCtorParam("title", o => o.MapFrom(x => x.Name))
                .ForCtorParam("synonymsCollection", o => o.MapFrom(x => x.Synonyms));

            CreateMap<MetadataSource, FileTagSource>()
                .ConvertUsing(
                    (x, _) => x switch
                    {
                        MetadataSource.Danbooru => FileTagSource.Danbooru,
                        MetadataSource.Yandere => FileTagSource.Yandere,
                        MetadataSource.Sankaku => FileTagSource.Sankaku,
                        MetadataSource.Manual => FileTagSource.Manual,
                        _ => throw new NotImplementedException(x.ToString())
                    });
        }
    }
}