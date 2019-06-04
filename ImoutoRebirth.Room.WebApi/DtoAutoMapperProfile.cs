using System;
using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.WebApi.Requests;
using ImoutoRebirth.Room.WebApi.Responses;

namespace ImoutoRebirth.Room.WebApi
{
    public class DtoAutoMapperProfile : Profile
    {
        public DtoAutoMapperProfile()
        {
            CreateMap<Collection, CollectionResponse>();
            CreateMap<SourceFolder, SourceFolderResponse>();
            CreateMap<CustomDestinationFolder, DestinationFolderResponse>()
               .ForCtorParam("path", o => o.MapFrom(x => x.GetDestinationDirectory().FullName));

            CreateMap<(Guid collectionId, SourceFolderCreateRequest createRequest), SourceFolderCreateData>()
               .ConvertUsing(x
                    => new SourceFolderCreateData(
                        x.collectionId, 
                        x.createRequest.Path,
                        x.createRequest.ShouldCheckFormat,
                        x.createRequest.ShouldCheckHashFromName,
                        x.createRequest.ShouldCreateTagsFromSubfolders,
                        x.createRequest.ShouldAddTagFromFilename,
                        x.createRequest.SupportedExtensions));

            CreateMap<(Guid collectionId, DestinationFolderCreateRequest createRequest), DestinationFolderCreateData>()
               .ConvertUsing(x
                    => new DestinationFolderCreateData(
                        x.collectionId, 
                        x.createRequest.Path,
                        x.createRequest.ShouldCreateSubfoldersByHash,
                        x.createRequest.ShouldRenameByHash,
                        x.createRequest.FormatErrorSubfolder,
                        x.createRequest.HashErrorSubfolder,
                        x.createRequest.WithoutHashErrorSubfolder));

            CreateMap<CollectionCreateRequest, CollectionCreateData>();
        }
    }
}