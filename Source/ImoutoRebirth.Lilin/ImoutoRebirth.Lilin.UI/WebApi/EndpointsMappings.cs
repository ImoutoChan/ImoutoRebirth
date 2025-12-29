using ImoutoRebirth.Lilin.Application.FileInfoSlice.Commands;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;
using ImoutoRebirth.Lilin.Application.TagSlice;
using ImoutoRebirth.Lilin.Application.TagTypeSlice;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace ImoutoRebirth.Lilin.UI.WebApi;

internal static class EndpointsMappings
{
    public static void MapFilesEndpoints(this WebApplication app)
    {
        var files = app.MapGroup("/files");

        files.MapGet("/{fileId:guid}", ([FromRoute] Guid fileId, IMediator mediator, CancellationToken ct)
                => mediator.Send(new FileInfoQuery(fileId), ct))
            .WithName("GetFileInfo");

        files.MapGet("/{fileMd5Hash}/relatives", (string fileMd5Hash, IMediator mediator, CancellationToken ct)
                => mediator.Send(new RelativesQuery(fileMd5Hash), ct))
            .WithName("GetRelativesInfo");

        files.MapPost("/relatives", (IReadOnlyCollection<string> fileMd5Hashes, IMediator mediator, CancellationToken ct)
                => mediator.Send(new RelativesBatchQuery(fileMd5Hashes), ct))
            .WithName("GetRelativesInfoBatch");

        files.MapPost("/search-fast", (SearchFilesFastQuery query, IMediator mediator, CancellationToken ct)
                => mediator.Send(query, ct))
            .WithName("SearchFilesFast");

        files.MapPost("/search-fast/count", (SearchFilesFastCountQuery query, IMediator mediator, CancellationToken ct)
                => mediator.Send(query, ct))
            .WithName("CountSearchFilesFast");

        files.MapPost("/filter", (FilterFilesQuery query, IMediator mediator, CancellationToken ct)
                => mediator.Send(query, ct))
            .WithName("FilterFiles");

        files.MapPost("/filter/count", (FilterFilesCountQuery query, IMediator mediator, CancellationToken ct)
                => mediator.Send(query, ct))
            .WithName("CountFilterFiles");

        files.MapPost("/tags", (BindTagsCommand command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct))
            .WithName("BindTags");

        files.MapDelete("/tags", ([FromBody] UnbindTagsCommand command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct))
            .WithName("UnbindTags");
    }

    public static void MapTagsEndpoints(this WebApplication app)
    {
        var tags = app.MapGroup("/tags");

        tags.MapPost("/search", (TagsSearchQuery query, IMediator mediator, CancellationToken ct)
            => mediator.Send(query, ct))
            .WithName("SearchTags");

        tags.MapPost("/values/search", (TagValuesSearchQuery query, IMediator mediator, CancellationToken ct)
            => mediator.Send(query, ct))
            .WithName("SearchTagValues");

        tags.MapGet("/popular", (int limit, IMediator mediator, CancellationToken ct)
            => mediator.Send(new PopularUserTagsQuery(limit), ct))
            .WithName("GetPopularTags");

        tags.MapGet("/popular-characters", (int limit, IMediator mediator, CancellationToken ct)
            => mediator.Send(new PopularUserCharacterTagsQuery(limit), ct))
            .WithName("GetPopularCharactersTags");

        tags.MapPost("", (CreateTagCommand command, IMediator mediator, CancellationToken ct)
            => mediator.Send(command, ct))
            .WithName("CreateTag");

        tags.MapGet("/types", (IMediator mediator, CancellationToken ct) 
            => mediator.Send(new TagTypesQuery(), ct))
            .WithName("GetTagTypes");
        
        tags.MapPost("/merge", (MergeTagsCommand command, IMediator mediator, CancellationToken ct)
            => mediator.Send(command, ct))
            .WithName("MergeTags");
        
        tags.MapDelete("/{tagId:guid}", (Guid tagId, IMediator mediator, CancellationToken ct)
            => mediator.Send(new DeleteTagCommand(tagId), ct))
            .WithName("DeleteTag");
    }
}
