using AutoMapper;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.WebApi.Responses;

namespace ImoutoRebirth.Lilin.WebApi
{
    public class DtoAutoMapperProfile : Profile
    {
        public DtoAutoMapperProfile()
        {
            CreateMap<FileInfo, FileInfoResponse>();
            CreateMap<FileTag, FileTagResponse>();
            CreateMap<FileNote, FileNoteResponse>();
            CreateMap<Note, NoteResponse>();
            CreateMap<Tag, TagResponse>();
            CreateMap<TagType, TagTypeResponse>();
        }
    }
}