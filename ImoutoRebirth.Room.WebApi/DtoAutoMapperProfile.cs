using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.WebApi.Responses;

namespace ImoutoRebirth.Room.WebApi
{
    public class DtoAutoMapperProfile : Profile
    {
        public DtoAutoMapperProfile()
        {
            CreateMap<Collection, CollectionResponse>();
        }
    }
}
