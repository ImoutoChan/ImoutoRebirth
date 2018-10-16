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
               .ConvertUsing(mappingFunction: x 
                    => new SourceFolderCreateData(
                        collectionId: x.collectionId, 
                        path: x.createRequest.Path,
                        shouldCheckFormat: x.createRequest.ShouldCheckFormat,
                        shouldCheckHashFromName: x.createRequest.ShouldCheckHashFromName,
                        shouldCreateTagsFromSubfolders: x.createRequest.ShouldCreateTagsFromSubfolders,
                        shouldAddTagFromFilename: x.createRequest.ShouldAddTagFromFilename,
                        supportedExtensions: x.createRequest.SupportedExtensions));

            CreateMap<(Guid collectionId, DestinationFolderCreateRequest createRequest), DestinationFolderCreateData>()
               .ConvertUsing(mappingFunction: x 
                    => new DestinationFolderCreateData(
                        collectionId: x.collectionId, 
                        path: x.createRequest.Path,
                        shouldCreateSubfoldersByHash: x.createRequest.ShouldCreateSubfoldersByHash,
                        shouldRenameByHash: x.createRequest.ShouldRenameByHash,
                        formatErrorSubfolder: x.createRequest.FormatErrorSubfolder,
                        hashErrorSubfolder: x.createRequest.HashErrorSubfolder,
                        withoutHashErrorSubfolder: x.createRequest.WithoutHashErrorSubfolder));
        }
    }
}