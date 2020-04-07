using AutoMapper;
using ImoutoRebirth.Room.WebApi.Client.Models;

namespace ImoutoRebirth.Navigator.Services.Collections
{
    internal class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CollectionResponse, Collection>();

            CreateMap<DestinationFolderResponse, DestinationFolder>();

            CreateMap<DestinationFolder, DestinationFolderCreateRequest>();

            CreateMap<SourceFolderResponse, SourceFolder>();

            CreateMap<SourceFolder, SourceFolderCreateRequest>();
        }
    }
}