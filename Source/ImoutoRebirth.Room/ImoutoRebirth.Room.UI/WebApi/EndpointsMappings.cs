using ImoutoRebirth.Room.Application.Cqrs;
using ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;
using ImoutoRebirth.Room.Application.Cqrs.CollectionSlice;
using ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;
using MediatR;
using Microsoft.AspNetCore.Builder;

namespace ImoutoRebirth.Room.UI.WebApi;

public record CollectionResponse(Guid Id, string Name);

internal static class EndpointsMappings
{
    public static void MapCollectionsEndpoints(this WebApplication app)
    {
        var collections = app.MapGroup("/collections");

        collections.MapGet("", async (AllCollectionsQuery query, IMediator mediator, CancellationToken ct)
                =>
                {
                    var found = await mediator.Send(query, ct);
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

        files.MapPost("", (CollectionFilesModelsQuery query, IMediator mediator, CancellationToken ct) 
                => mediator.Send(query, ct))
            .WithName("SearchCollectionFiles");
        
        files.MapPost("filter-hashes", (FilterCollectionFileHashesQuery command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct))
            .WithName("FilterCollectionFileHashes");
        
        files.MapPost("search-ids", (CollectionFilesIdsQuery query, IMediator mediator, CancellationToken ct) 
                => mediator.Send(query, ct))
            .WithName("SearchCollectionFileIds");
        
        files.MapPost("count", (CollectionFilesCountQuery query, IMediator mediator, CancellationToken ct) 
                => mediator.Send(query, ct))
            .WithName("CountCollectionFiles");
        
        files.MapPost("updateSourceTags", (UpdateLocationTagsCommand command, IMediator mediator, CancellationToken ct) 
                => mediator.Send(command, ct))
            .WithName("UpdateSourceTags");

        files.MapDelete("/{id:guid}", (Guid id, IMediator mediator, CancellationToken ct)
                => mediator.Send(new DeleteCollectionFileCommand(id), ct))
            .WithName("DeleteCollectionFile");
    }

    public static void MapDestinationFoldersEndpoints(this WebApplication app)
    {
        var destinationFolders = app.MapGroup("/collections/destination-folders");

        destinationFolders.MapGet("/{collectionId:guid}", (Guid collectionId, IMediator mediator, CancellationToken ct) 
                => mediator.Send(new DestinationFolderQuery(collectionId), ct))
            .WithName("GetDestinationFolder");
        
        destinationFolders.MapPost("", (SetDestinationFolderCommand command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct))
            .WithName("SetDestinationFolder");

        destinationFolders.MapDelete("", (DeleteDestinationFolderCommand command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct))
            .WithName("DeleteDestinationFolder");
    }

    public static void MapSourceFoldersEndpoints(this WebApplication app)
    {
        var sourceFolders = app.MapGroup("/collections/source-folders");

        sourceFolders.MapGet("/{collectionId:guid}", (Guid collectionId, IMediator mediator, CancellationToken ct) 
                => mediator.Send(new SourceFoldersQuery(collectionId), ct))
            .WithName("GetSourceFolders");
        
        sourceFolders.MapPost("", (AddSourceFolderCommand command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct))
            .WithName("AddSourceFolder");
        
        sourceFolders.MapPut("", (UpdateSourceFolderCommand command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct))
            .WithName("UpdateSourceFolder");

        sourceFolders.MapDelete("", (DeleteSourceFolderCommand command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct))
            .WithName("DeleteSourceFolder");
    }
}
