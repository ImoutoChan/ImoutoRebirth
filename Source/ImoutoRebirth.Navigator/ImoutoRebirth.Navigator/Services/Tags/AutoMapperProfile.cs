﻿using AutoMapper;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.RoomService.WebApi.Client;
using FileTag = ImoutoRebirth.Navigator.Services.Tags.Model.FileTag;
using Tag = ImoutoRebirth.Navigator.Services.Tags.Model.Tag;
using LilinTag = ImoutoRebirth.LilinService.WebApi.Client.Tag;
using LilinFileTag = ImoutoRebirth.LilinService.WebApi.Client.DetailedFileTag;
using LilinTagType = ImoutoRebirth.LilinService.WebApi.Client.TagType;
using TagType = ImoutoRebirth.Navigator.Services.Tags.Model.TagType;

namespace ImoutoRebirth.Navigator.Services.Tags;

internal class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<SearchTag, TagSearchEntry>()
            .ForCtorParam("tagId", o => o.MapFrom(x => x.Tag.Id))
            .ForCtorParam("value", o => o.MapFrom(x => x.Value))
            .ForCtorParam("tagSearchScope", o => o.MapFrom(x => x.SearchType));

        CreateMap<FileTag, BindTag>()
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

        CreateMap<FileTagSource, MetadataSource>()
            .ConvertUsing(
                (x, _) => x switch
                {
                    FileTagSource.Yandere => MetadataSource.Yandere,
                    FileTagSource.Danbooru => MetadataSource.Danbooru,
                    FileTagSource.Sankaku => MetadataSource.Sankaku,
                    FileTagSource.Manual => MetadataSource.Manual,
                    FileTagSource.Gelbooru => MetadataSource.Gelbooru,
                    _ => throw new NotImplementedException(x.ToString())
                });

        CreateMap<CollectionFileResponse, File>();

        CreateFileTagMaps();
        CreateTagMaps();
    }

    private void CreateTagMaps()
    {
        CreateMap<LilinTag, Tag>()
            .ForCtorParam("title", o => o.MapFrom(x => x.Name))
            .ForCtorParam("synonymsCollection", o => o.MapFrom(x => x.Synonyms));
        // CreateMap<FileTagResponseTag, Tag>()
        //     .ForCtorParam("title", o => o.MapFrom(x => x.Name))
        //     .ForCtorParam("synonymsCollection", o => o.MapFrom(x => x.Synonyms));

        CreateMap<LilinTagType, TagType>()
            .ForCtorParam("title", o => o.MapFrom(x => x.Name));
        // CreateMap<TagResponseType, TagType>()
        //     .ForCtorParam("title", o => o.MapFrom(x => x.Name));
    }

    private void CreateFileTagMaps()
    {
        CreateMap<LilinFileTag, FileTag>();

        CreateMap<MetadataSource, FileTagSource>()
            .ConvertUsing(
                (x, _) => x switch
                {
                    MetadataSource.Danbooru => FileTagSource.Danbooru,
                    MetadataSource.Yandere => FileTagSource.Yandere,
                    MetadataSource.Sankaku => FileTagSource.Sankaku,
                    MetadataSource.Manual => FileTagSource.Manual,
                    MetadataSource.Gelbooru => FileTagSource.Gelbooru,
                    _ => throw new NotImplementedException(x.ToString())
                });
    }
}
