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
            CreateMap<CustomDestinationFolder, DestinationFolderEntity>();
            CreateMap<SourceFolder, SourceFolderEntity>();

            CreateMap<CollectionCreateData, Collection>()
               .ConstructUsing(x => new Collection(Guid.NewGuid(), x.Name));
        }
    }
}
