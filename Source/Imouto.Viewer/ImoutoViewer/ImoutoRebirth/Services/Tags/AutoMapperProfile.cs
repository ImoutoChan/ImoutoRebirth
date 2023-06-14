using AutoMapper;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.RoomService.WebApi.Client;
using ImoutoViewer.ImoutoRebirth.NavigatorArgs;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;
using FileNote = ImoutoRebirth.LilinService.WebApi.Client.FileNote;
using FileTag = ImoutoRebirth.LilinService.WebApi.Client.DetailedFileTag;
using LocalFileTag = ImoutoViewer.ImoutoRebirth.Services.Tags.Model.FileTag;
using LocalTag = ImoutoViewer.ImoutoRebirth.Services.Tags.Model.Tag;
using LocalTagType = ImoutoViewer.ImoutoRebirth.Services.Tags.Model.TagType;
using LocalNote = ImoutoViewer.ImoutoRebirth.Services.Tags.Model.Note;
using LocalFileNote = ImoutoViewer.ImoutoRebirth.Services.Tags.Model.FileNote;
using Note = ImoutoRebirth.LilinService.WebApi.Client.FileNote;
using SearchType = ImoutoViewer.ImoutoRebirth.Services.Tags.Model.SearchType;
using Tag = ImoutoRebirth.LilinService.WebApi.Client.Tag;
using TagType = ImoutoRebirth.LilinService.WebApi.Client.TagType;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags;

internal class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<SearchTag, TagSearchEntry>()
            .ForCtorParam("tagId", o => o.MapFrom(x => x.TagId))
            .ForCtorParam("value", o => o.MapFrom(x => x.Value))
            .ForCtorParam("tagSearchScope", o => o.MapFrom(x => x.SearchType));

        CreateMap<LocalFileTag, FileTag>();

        CreateMap<SearchType, TagSearchScope>()
            .ConvertUsing(
                (x, _) => x switch
                {
                    SearchType.Include => TagSearchScope.Included,
                    SearchType.Exclude => TagSearchScope.Excluded,
                    _ => throw new NotImplementedException(x.ToString())
                });

        CreateMap<FileNote, LocalFileNote>();

        CreateMap<Note, LocalNote>();

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
        CreateMap<Tag, LocalTag>()
            .ForCtorParam("title", o => o.MapFrom(x => x.Name))
            .ForCtorParam("synonymsCollection", o => o.MapFrom(x => x.Synonyms));

        CreateMap<TagType, LocalTagType>()
            .ForCtorParam("title", o => o.MapFrom(x => x.Name));
    }

    private void CreateFileTagMaps()
    {
        CreateMap<FileTag, LocalFileTag>();

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
