using AutoMapper;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using ImoutoRebirth.Lilin.Services.CQRS.Commands;
using ImoutoRebirth.Lilin.WebApi.Requests;
using ImoutoRebirth.Lilin.WebApi.Responses;

namespace ImoutoRebirth.Lilin.WebApi
{
    public class DtoAutoMapperProfile : Profile
    {
        public DtoAutoMapperProfile()
        {
            CreateMapFromModelToResponses();
            CreateMapFromRequestsToQueries();
        }

        private void CreateMapFromRequestsToQueries()
        {
            CreateMap<TagCreateRequest, CreateTagCommand>();
        }

        private void CreateMapFromModelToResponses()
        {
            CreateMap<FileInfo, FileInfoResponse>();
            CreateMap<FileTag, FileTagResponse>();
            CreateMap<FileNote, FileNoteResponse>();
            CreateMap<Note, NoteResponse>();
            CreateMap<Tag, TagResponse>();
            CreateMap<TagType, TagTypeResponse>();
            CreateMap<RelativeInfo, RelativeResponse>();
        }
    }
}