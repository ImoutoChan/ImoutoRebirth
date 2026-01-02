using ImoutoRebirth.Common.WebApi;
using ImoutoRebirth.Room.Application.Cqrs;
using ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;
using ImoutoRebirth.Room.Application.Cqrs.CollectionSlice;
using ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;
using ImoutoRebirth.Room.Application.Cqrs.IntegritySlice;
using MediatR;
using Microsoft.AspNetCore.Builder;

namespace ImoutoRebirth.Room.UI.WebApi;

public record CollectionResponse(Guid Id, string Name);

internal static class EndpointsMappings
{
    public static void MapCollectionsEndpoints(this WebApplication app)
    {
        var collections = app.MapGroup("/collections");

        collections.MapGet("", async (IMediator mediator, CancellationToken ct)
                =>
                {
                    var found = await mediator.Send(new AllCollectionsQuery(), ct);
                    return found.Select(x => new CollectionResponse(x.Id, x.Name)).ToList();
                })
            .WithName("GetAllCollections");
        
        collections.MapPost("", (CreateCollectionCommand command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct))
            .WithName("CreateCollection");

        collections.MapPatch("/{collectionId:guid}", (Guid collectionId, string newName, IMediator mediator, CancellationToken ct)
                => mediator.Send(new RenameCollectionCommand(collectionId, newName), ct))
            .WithName("RenameCollection");

        collections.MapDelete("/{collectionId:guid}", (Guid collectionId, IMediator mediator, CancellationToken ct)
                => mediator.Send(new DeleteCollectionCommand(collectionId), ct))
            .WithName("DeleteCollection");
    }

    public static void MapCollectionFilesEndpoints(this WebApplication app)
    {
        var files = app.MapGroup("/collection-files");

        files.MapPost("", (CollectionFilesQuery query, IMediator mediator, CancellationToken ct) 
                => mediator.Send(new CollectionFilesModelsQuery(query), ct))
            .WithName("SearchCollectionFiles");
        
        files.MapPost("filter-hashes", (FilterCollectionFileHashesQuery command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct))
            .WithName("FilterCollectionFileHashes");
        
        files.MapPost("search-ids", (CollectionFilesQuery query, IMediator mediator, CancellationToken ct) 
                => mediator.Send(new CollectionFilesIdsQuery(query), ct))
            .WithName("SearchCollectionFileIds");
        
        files.MapPost("count", (CollectionFilesQuery query, IMediator mediator, CancellationToken ct) 
                => mediator.Send(new CollectionFilesCountQuery(query), ct))
            .WithName("CountCollectionFiles");
        
        files.MapPost("update-source-tags", (IMediator mediator, CancellationToken ct)
                => mediator.Send(new UpdateLocationTagsCommand(), ct))
            .WithName("UpdateSourceTags");

        files.MapDelete("{id:guid}", (Guid id, IMediator mediator, CancellationToken ct)
                => mediator.Send(new DeleteCollectionFileCommand(id), ct))
            .WithName("DeleteCollectionFile");

        files.MapGet("{id:guid}", (Guid id, IMediator mediator, CancellationToken ct)
                => mediator.Send(new CollectionFileMetadataQuery(id), ct))
            .WithName("GetCollectionFileMetadata");

        files.MapPost("update-file-metadata", (IMediator mediator, CancellationToken ct)
                => mediator.Send(new UpdateFileMetadataCommand(), ct))
            .WithName("UpdateFileMetadata");

        files.MapPatch("{id:guid}/rename", (Guid id, string newFileName, IMediator mediator, CancellationToken ct)
                => mediator.Send(new RenameCollectionFileCommand(id, newFileName), ct))
            .WithName("RenameCollectionFile");

        files.MapGet("{id:guid}/renaming-availability", (Guid id, string newFileName, IMediator mediator, CancellationToken ct)
                => mediator.Send(new CanRenameCollectionFileToCommand(id, newFileName), ct))
            .WithName("CanRenameCollectionFile");
    }

    public static void MapIntegrityReportsEndpoints(this WebApplication app)
    {
        var files = app.MapGroup("/integrity-reports");

        files.MapPost("", (CreateIntegrityReportJobCommand command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct))
            .WithName("BuildIntegrityReport");

        files.MapGet("", (IMediator mediator, CancellationToken ct, int count = 10, int skip = 0)
                => mediator.Send(new IntegrityReportsQuery(count, skip), ct))
            .WithName("GetIntegrityReports");

        files.MapPut("/{reportId:guid}/pause", (Guid reportId, IMediator mediator, CancellationToken ct)
                    => mediator.Send(new PauseIntegrityReportJobCommand(reportId), ct))
            .WithName("PauseIntegrityReport");

        files.MapPut("/{reportId:guid}/resume", (Guid reportId, IMediator mediator, CancellationToken ct)
                    => mediator.Send(new ResumeIntegrityReportJobCommand(reportId), ct))
            .WithName("ResumeIntegrityReport");
    }

    public static void MapDestinationFoldersEndpoints(this WebApplication app)
    {
        var destinationFolders = app.MapGroup("/collections");

        destinationFolders.MapGet("/{collectionId:guid}/destination-folder", (Guid collectionId, IMediator mediator, CancellationToken ct) 
                => mediator.Send(new DestinationFolderQuery(collectionId), ct).ToOptional())
            .WithName("GetDestinationFolder");
        
        destinationFolders.MapPost("/destination-folder", (SetDestinationFolderCommand command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct))
            .WithName("SetDestinationFolder");

        destinationFolders.MapDelete("/{collectionId:guid}/destination-folder", (Guid collectionId, IMediator mediator, CancellationToken ct)
                => mediator.Send(new DeleteDestinationFolderCommand(collectionId), ct))
            .WithName("DeleteDestinationFolder");
    }

    public static void MapSourceFoldersEndpoints(this WebApplication app)
    {
        var sourceFolders = app.MapGroup("/collections");

        sourceFolders.MapGet("/{collectionId:guid}/source-folders", (Guid collectionId, IMediator mediator, CancellationToken ct) 
                => mediator.Send(new SourceFoldersQuery(collectionId), ct))
            .WithName("GetSourceFolders");
        
        sourceFolders.MapPost("/source-folders", (AddSourceFolderCommand command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct))
            .WithName("AddSourceFolder");
        
        sourceFolders.MapPut("/source-folders", (UpdateSourceFolderCommand command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct))
            .WithName("UpdateSourceFolder");

        sourceFolders.MapDelete("/{collectionId:guid}/source-folders/{sourceFolderId:guid}", (Guid collectionId, Guid sourceFolderId, IMediator mediator, CancellationToken ct)
                => mediator.Send(new DeleteSourceFolderCommand(collectionId, sourceFolderId), ct))
            .WithName("DeleteSourceFolder");
    }
}
