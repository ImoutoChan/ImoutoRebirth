using AutoMapper;
using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.Database.Entities;

namespace ImoutoRebirth.Room.Core
{
    public class ModelAutoMapperProfile : Profile
    {
        public ModelAutoMapperProfile()
        {
            CreateMap<CollectionEntity, Collection>();
            CreateMap<CollectionFileEntity, CollectionFile>();
            CreateMap<DestinationFolderEntity, DestinationFolder>();
            CreateMap<SourceFolderEntity, SourceFolder>();

            CreateMap<Collection, CollectionEntity>();
            CreateMap<CollectionFile, CollectionFileEntity>();
            CreateMap<DestinationFolder, DestinationFolderEntity>();
            CreateMap<SourceFolder, SourceFolderEntity>();
        }
    }
}
