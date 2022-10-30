using AutoMapper;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.RoomService.WebApi.Client;
using ImoutoViewer.ImoutoRebirth.NavigatorArgs;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;
using SearchType = ImoutoViewer.ImoutoRebirth.Services.Tags.Model.SearchType;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags;

internal class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<SearchTag, TagSearchEntryRequest>()
            .ForMember(x => x.TagId, o => o.MapFrom(x => x.TagId))
            .ForMember(x => x.Value, o => o.MapFrom(x => x.Value))
            .ForMember(x => x.TagSearchScope, o => o.MapFrom(x => x.SearchType));

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

        CreateMap<FileNoteResponse, FileNote>();

        CreateMap<NoteResponse, Note>();

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

        CreateMap<FileTagRequestSource, FileTagSource>()
            .ConvertUsing(
                (x, _) => x switch
                {
                    FileTagRequestSource.Yandere => FileTagSource.Yandere,
                    FileTagRequestSource.Danbooru => FileTagSource.Danbooru,
                    FileTagRequestSource.Sankaku => FileTagSource.Sankaku,
                    FileTagRequestSource.Manual => FileTagSource.Manual,
                    _ => throw new NotImplementedException(x.ToString())
                });

        CreateMap<CollectionFileResponse, File>();

        CreateMap<SearchTagDto, SearchTag>();

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

        CreateMap<FileTagResponseSource, FileTagSource>()
            .ConvertUsing(
                (x, _) => x switch
                {
                    FileTagResponseSource.Danbooru => FileTagSource.Danbooru,
                    FileTagResponseSource.Yandere => FileTagSource.Yandere,
                    FileTagResponseSource.Sankaku => FileTagSource.Sankaku,
                    FileTagResponseSource.Manual => FileTagSource.Manual,
                    _ => throw new NotImplementedException(x.ToString())
                });
    }
}
