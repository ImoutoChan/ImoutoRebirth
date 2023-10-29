using System.Text.RegularExpressions;
using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Lilin.Domain.FileInfoAggregate;
using Microsoft.Extensions.Caching.Memory;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice;

internal partial class FileInfoUpdatedCacheResetDomainEventHandler 
    : DomainEventNotificationHandler<FileInfoUpdatedDomainEvent>
{
    private readonly IMemoryCache _memoryCache;

    public FileInfoUpdatedCacheResetDomainEventHandler(IMemoryCache memoryCache) => _memoryCache = memoryCache;

    protected override Task Handle(FileInfoUpdatedDomainEvent domainEvent, CancellationToken ct)
    {
        var md5Regex = Md5Regex();

        var relativeMd5 = domainEvent.Aggregate.Tags
            .Where(x => x.Value != null)
            .SelectMany(x => md5Regex.Matches(x.Value!))
            .Select(x => x.Value)
            .Distinct()
            .ToList();

        foreach (var md5 in relativeMd5) 
            _memoryCache.Remove(GetKey(md5));

        return Task.CompletedTask;
    }
    
    private static string GetKey(string md5Hash) => "FilterHashes_" + md5Hash.ToLowerInvariant();

    [GeneratedRegex("[a-f0-9]{32}", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex Md5Regex();
}
