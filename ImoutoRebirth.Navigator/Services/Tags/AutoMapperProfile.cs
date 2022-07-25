using AutoMapper;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Room.WebApi.Client.Models;

namespace ImoutoRebirth.Navigator.Services.Tags;

internal class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<SearchTag, TagSearchEntryRequest>()
            .ForCtorParam("TagId", o => o.MapFrom(x => x.Tag.Id))
            .ForCtorParam("Value", o => o.MapFrom(x => x.Value))
            .ForCtorParam("TagSearchScope", o => o.MapFrom(x => x.SearchType));

        CreateMap<FileTag, FileTagRequest>()
            .ForMember(x => x.TagId, o => o.MapFrom(x => x.Tag.Id))
            .ForMember(x => x.Value, o => o.MapFrom(x => x.Value));

        CreateMap<SearchType, TagSearchEntryRequestTagSearchScope>()
            .ConvertUsing(
                (x, _) => x switch
                {
                    SearchType.Include => TagSearchEntryRequestTagSearchScope.Included,
                    SearchType.Exclude => TagSearchEntryRequestTagSearchScope.Excluded,
                    _ => throw new NotImplementedException(x.ToString())
                });

        CreateMap<FileTagSource, FileTagRequestSource>()
            .ConvertUsing(
                (x, _) => x switch
                {
                    FileTagSource.Yandere => FileTagRequestSource.Yandere,
                    FileTagSource.Danbooru => FileTagRequestSource.Danbooru,
                    FileTagSource.Sankaku => FileTagRequestSource.Sankaku,
                    FileTagSource.Manual => FileTagRequestSource.Manual,
                    _ => throw new NotImplementedException(x.ToString())
                });

        CreateMap<CollectionFileResponse, File>();

        CreateFileTagMaps();
        CreateTagMaps();
    }

    private void CreateTagMaps()
    {
        CreateMap<TagResponse, Tag>()
            .ForCtorParam("title", o => o.MapFrom(x => x.Name))
            .ForCtorParam("synonymsCollection", o => o.MapFrom(x => x.Synonyms));
        // CreateMap<FileTagResponseTag, Tag>()
        //     .ForCtorParam("title", o => o.MapFrom(x => x.Name))
        //     .ForCtorParam("synonymsCollection", o => o.MapFrom(x => x.Synonyms));

        CreateMap<TagTypeResponse, TagType>()
            .ForCtorParam("title", o => o.MapFrom(x => x.Name));
        // CreateMap<TagResponseType, TagType>()
        //     .ForCtorParam("title", o => o.MapFrom(x => x.Name));
    }

    private void CreateFileTagMaps()
    {
        CreateMap<FileTagResponse, FileTag>();

        CreateMap<FileTagRequestSource, FileTagSource>()
            .ConvertUsing(
                (x, _) => x switch
                {
                    FileTagRequestSource.Danbooru => FileTagSource.Danbooru,
                    FileTagRequestSource.Yandere => FileTagSource.Yandere,
                    FileTagRequestSource.Sankaku => FileTagSource.Sankaku,
                    FileTagRequestSource.Manual => FileTagSource.Manual,
                    _ => throw new NotImplementedException(x.ToString())
                });
    }
}
