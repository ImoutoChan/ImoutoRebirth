using System.Runtime.CompilerServices;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.TagSlice;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;

public record RelativesQuery(string Md5) : IQuery<IReadOnlyCollection<RelativeInfo>>;

public record RelativeInfo(RelativeType RelativesType, DetailedFileInfo FileInfo);

public enum RelativeType
{
    Child,
    Parent
}

internal class RelativesQueryHandler : IQueryHandler<RelativesQuery, IReadOnlyCollection<RelativeInfo>>
{
    private readonly IMediator _mediator;
    private readonly IMemoryCache _memoryCache;

    public RelativesQueryHandler(IMediator mediator, IMemoryCache memoryCache)
    {
        _mediator = mediator;
        _memoryCache = memoryCache;
    }

    public async Task<IReadOnlyCollection<RelativeInfo>> Handle(
        RelativesQuery request, 
        CancellationToken cancellationToken)
    {
        var md5 = request.Md5.ToLowerInvariant();
        var results = AsyncEnumerable.Empty<RelativeInfo>();

        results = results.Union(await LoadRelativeInfo("ParentMd5", RelativeType.Parent, md5, cancellationToken));
        results = results.Union(await LoadRelativeInfo("Child", RelativeType.Child, $"*{md5}", cancellationToken));
            
        return await results.ToArrayAsync(cancellationToken);
    }

    private async Task<IAsyncEnumerable<RelativeInfo>> LoadRelativeInfo(
        string tagName, 
        RelativeType type, 
        string md5,
        CancellationToken cancellationToken)
    {
        var tagId = await GetTagId(tagName, cancellationToken);
        if (!tagId.HasValue) 
            return AsyncEnumerable.Empty<RelativeInfo>();

        var relativeInfo = GetFileInfoByTagValueRelativeInfo(tagId.Value, md5, cancellationToken)
            .Select(x => new RelativeInfo(type, x));

        return relativeInfo;
    }

    private async IAsyncEnumerable<DetailedFileInfo> GetFileInfoByTagValueRelativeInfo(
        Guid tagId, 
        string? value, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var filesQuery = new SearchFilesQuery(
            new[] {new TagSearchEntry(tagId, value, TagSearchScope.Included)}, int.MaxValue, 0);

        var found = await _mediator.Send(filesQuery, cancellationToken);
            
        // todo read about cancellation in IAsyncEnumerable
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var guid in found)
        {
            var fileInfo = await _mediator.Send(new FileInfoQuery(guid), cancellationToken);
            yield return fileInfo;
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private async Task<Guid?> GetTagId(string name, CancellationToken cancellationToken)
    {
        return await _memoryCache.GetOrCreateAsync(
            name,
            async entry =>
            {
                var tagId = await SearchTag(name, cancellationToken);
                entry.AbsoluteExpirationRelativeToNow = tagId.HasValue ? TimeSpan.FromDays(1) : TimeSpan.Zero;
                return tagId;
            });

        async Task<Guid?> SearchTag(string tagName, CancellationToken token)
        {
            var result = await _mediator.Send(new TagsSearchQuery(tagName, Limit: 1), token);
            return result.Select(x => x.Id).SingleOrDefault();
        }
    }
}
