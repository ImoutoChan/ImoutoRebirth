using System;
using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.Database.Entities;

namespace ImoutoRebirth.Room.DataAccess
{
    public class ModelAutoMapperProfile : Profile
    {
        public ModelAutoMapperProfile()
        {
            CreateMap<CollectionEntity, Collection>();
            CreateMap<CollectionFileEntity, CollectionFile>();
            CreateMap<DestinationFolderEntity, CustomDestinationFolder>();
            CreateMap<SourceFolderEntity, SourceFolder>()
               .ForCtorParam("supportedExtensions", o => o.MapFrom(x => x.SupportedExtensionCollection));

            CreateMap<Collection, CollectionEntity>();
            CreateMap<CollectionFile, CollectionFileEntity>();
            CreateMap<CustomDestinationFolder, DestinationFolderEntity>()
               .ForMember(x => x.Path, o => o.MapFrom(x => x.GetDestinationDirectory()));
            CreateMap<SourceFolder, SourceFolderEntity>()
               .ForMember(x => x.SupportedExtensionCollection, o => o.MapFrom(x => x.SupportedExtensions));

            CreateMap<CollectionCreateData, Collection>()
               .ConstructUsing(x => new Collection(Guid.NewGuid(), x.Name));

            CreateMap<SourceFolderCreateData, SourceFolder>()
               .ForCtorParam("id", o => o.MapFrom(x => Guid.NewGuid()));

            CreateMap<DestinationFolderCreateData, CustomDestinationFolder>()
               .ForCtorParam("id", o => o.MapFrom(x => Guid.NewGuid()));
        }
    }
}