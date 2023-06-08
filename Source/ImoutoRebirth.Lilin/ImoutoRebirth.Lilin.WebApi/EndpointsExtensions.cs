using ImoutoRebirth.Lilin.Services.CQRS.Commands;
using ImoutoRebirth.Lilin.Services.CQRS.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;

namespace ImoutoRebirth.Lilin.WebApi;

internal static class EndpointsMappings
{
    public static void MapFilesEndpoints(this WebApplication app)
    {
        var files = app.MapGroup("/files");

        files.MapGet("/{fileId:guid}", (Guid fileId, IMediator mediator, CancellationToken ct)
                => mediator.Send(new FileInfoQuery(fileId), ct));

        files.MapGet("/{fileMd5Hash}/relatives", (string fileMd5Hash, IMediator mediator, CancellationToken ct)
                => mediator.Send(new RelativesQuery(fileMd5Hash), ct));

        files.MapPost(
            "/relatives",
            (IReadOnlyCollection<string> fileMd5Hashes, IMediator mediator, CancellationToken ct)
                => mediator.Send(new RelativesBatchQuery(fileMd5Hashes), ct));

        files.MapPost("/search", (FilesSearchQuery query, IMediator mediator, CancellationToken ct)
                => mediator.Send(query, ct));

        files.MapPost("/search/count", (FilesSearchQueryCount query, IMediator mediator, CancellationToken ct)
                => mediator.Send(query, ct));

        files.MapPost("/filter", (FilesFilterQuery query, IMediator mediator, CancellationToken ct)
                => mediator.Send(query, ct));

        files.MapPost("/tags", (BindTagsCommand command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct));

        files.MapDelete("/tags", (UnbindTagCommand command, IMediator mediator, CancellationToken ct)
                => mediator.Send(command, ct));
    }

    public static void MapTagsEndpoints(this WebApplication app)
    {
        var tags = app.MapGroup("/files");

        tags.MapPost("/search", (TagsSearchQuery query, IMediator mediator, CancellationToken ct)
            => mediator.Send(query, ct));

        tags.MapGet("/popular", (int limit, IMediator mediator, CancellationToken ct)
            => mediator.Send(new PopularUserTagsQuery(limit), ct));

        tags.MapPost("", (CreateTagCommand command, IMediator mediator, CancellationToken ct)
            => mediator.Send(command, ct));

        tags.MapGet("/types", (IMediator mediator, CancellationToken ct) 
            => mediator.Send(new TagTypesQuery(), ct));
    }
}
