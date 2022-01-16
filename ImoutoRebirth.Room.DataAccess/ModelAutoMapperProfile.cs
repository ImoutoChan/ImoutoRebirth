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

            CreateMap<Collection, CollectionEntity>()
                .ForMember(x => x.SourceFolders, o => o.Ignore())
                .ForMember(x => x.DestinationFolder, o => o.Ignore())
                .ForMember(x => x.Files, o => o.Ignore())
                .ForMember(x => x.AddedOn, o => o.Ignore())
                .ForMember(x => x.ModifiedOn, o => o.Ignore());


            CreateMap<CollectionFile, CollectionFileEntity>()
                .ForMember(x => x.IsRemoved, o => o.Ignore())
                .ForMember(x => x.Collection, o => o.Ignore())
                .ForMember(x => x.AddedOn, o => o.Ignore())
                .ForMember(x => x.ModifiedOn, o => o.Ignore());

            CreateMap<CustomDestinationFolder, DestinationFolderEntity>()
               .ForMember(x => x.Path, o => o.MapFrom(x => x.GetDestinationDirectory().FullName))
               .ForMember(x => x.Collection, o => o.Ignore())
               .ForMember(x => x.AddedOn, o => o.Ignore())
               .ForMember(x => x.ModifiedOn, o => o.Ignore());

            CreateMap<SourceFolder, SourceFolderEntity>()
               .ForMember(x => x.SupportedExtensionCollection, o => o.MapFrom(x => x.SupportedExtensions))
               .ForMember(x => x.Collection, o => o.Ignore())
               .ForMember(x => x.AddedOn, o => o.Ignore())
               .ForMember(x => x.ModifiedOn, o => o.Ignore());

            CreateMap<CollectionCreateData, Collection>()
               .ConstructUsing(x => new Collection(Guid.NewGuid(), x.Name));

            CreateMap<SourceFolderCreateData, SourceFolder>()
               .ForCtorParam("id", o => o.MapFrom(x => Guid.NewGuid()));

            CreateMap<DestinationFolderCreateData, CustomDestinationFolder>()
               .ForCtorParam("id", o => o.MapFrom(x => Guid.NewGuid()));
        }
    }
}