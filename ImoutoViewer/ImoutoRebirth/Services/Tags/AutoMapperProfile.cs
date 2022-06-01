using AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client.Models;
using ImoutoRebirth.Room.WebApi.Client.Models;
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

        CreateMap<SearchType, TagSearchScope>()
            .ConvertUsing(
                (x, _) => x switch
                {
                    SearchType.Include => TagSearchScope.Included,
                    SearchType.Exclude => TagSearchScope.Excluded,
                    _ => throw new NotImplementedException(x.ToString())
                });

        CreateMap<FileNoteResponse, FileNote>();

        CreateMap<FileNoteResponseNote, Note>();

        CreateMap<FileTagSource, MetadataSource>()
            .ConvertUsing(
                (x, _) => x switch
                {
                    FileTagSource.Yandere => MetadataSource.Yandere,
                    FileTagSource.Danbooru => MetadataSource.Danbooru,
                    FileTagSource.Sankaku => MetadataSource.Sankaku,
                    FileTagSource.Manual => MetadataSource.Manual,
                    _ => throw new NotImplementedException(x.ToString())
                });

        CreateMap<MetadataSource, FileTagSource>()
            .ConvertUsing(
                (x, _) => x switch
                {
                     MetadataSource.Yandere => FileTagSource.Yandere,
                     MetadataSource.Danbooru => FileTagSource.Danbooru,
                     MetadataSource.Sankaku => FileTagSource.Sankaku,
                     MetadataSource.Manual => FileTagSource.Manual,
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
        CreateMap<FileTagResponseTag, Tag>()
            .ForCtorParam("title", o => o.MapFrom(x => x.Name))
            .ForCtorParam("synonymsCollection", o => o.MapFrom(x => x.Synonyms));

        CreateMap<TagTypeResponse, TagType>()
            .ForCtorParam("title", o => o.MapFrom(x => x.Name));
        CreateMap<TagResponseType, TagType>()
            .ForCtorParam("title", o => o.MapFrom(x => x.Name));
    }

    private void CreateFileTagMaps()
    {
        CreateMap<FileTagResponse, FileTag>();

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
