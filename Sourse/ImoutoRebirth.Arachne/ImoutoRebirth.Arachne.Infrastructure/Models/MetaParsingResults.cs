using ImoutoRebirth.Arachne.Core.Models;
using Newtonsoft.Json;

namespace ImoutoRebirth.Arachne.Infrastructure.Models;

public record FrameData(int Delay, string File);

public record UgoiraFrameData(string ContentType, IReadOnlyCollection<FrameData> Data);

internal class MetaParsingResults
{
    public string? Source { get; }

    public string BooruPostId { get; }
        
    public string BooruLastUpdate { get; }

    public string Md5 { get; }

    public string PostedDateTime { get; }

    public string? PostedById { get; }

    public string? PostedByUsername { get; }

    public string Rating { get; }
    
    public string RatingSafeLevel { get; }

    public string? ParentId { get; }

    public string? ParentMd5 { get; }

    public IReadOnlyCollection<string> Childs { get; }

    public IReadOnlyCollection<string> Pools { get; }

    public IReadOnlyCollection<MetaParsingTagResults> Tags { get; }

    public IReadOnlyCollection<Note> Notes { get; }

    public UgoiraFrameData? UgoiraFrameData { get; }

    public MetaParsingResults(
        string? source,
        string booruPostId,
        string booruLastUpdate,
        string md5,
        string postedDateTime,
        string? postedById,
        string? postedByUsername,
        string rating,
        string ratingSafeLevel,
        string? parentId,
        string? parentMd5,
        IReadOnlyCollection<string> childs,
        IReadOnlyCollection<string> pools,
        IReadOnlyCollection<MetaParsingTagResults> tags,
        IReadOnlyCollection<Note> notes,
        UgoiraFrameData? ugoiraFrameData)
    {
        Source = source;
        BooruPostId = booruPostId;
        BooruLastUpdate = booruLastUpdate;
        Md5 = md5;
        PostedDateTime = postedDateTime;
        PostedById = postedById;
        PostedByUsername = postedByUsername;
        Rating = rating;
        RatingSafeLevel = ratingSafeLevel;
        ParentId = parentId;
        ParentMd5 = parentMd5;
        Childs = childs;
        Pools = pools;
        Tags = tags;
        Notes = notes;
        UgoiraFrameData = ugoiraFrameData;
    }

    public IEnumerable<(string TagType, string Tag, string Value)> GetMetaTags()
    {
        const string tagType = "LocalMeta";

        var props = typeof(MetaParsingResults).GetProperties();
        foreach (var propertyInfo in props)
        {
            var tag = propertyInfo.Name;

            if (tag is nameof(Tags) or nameof(Notes))
                continue;

            switch (propertyInfo.GetValue(this))
            {
                case string strValue:
                    yield return (tagType, tag, strValue);
                    break;
                case UgoiraFrameData ugoira:
                    yield return (tagType, tag, JsonConvert.SerializeObject(ugoira));
                    break;
                case IReadOnlyCollection<string> collectionValue:
                    tag = tag.Trim('s');
                    foreach (var value in collectionValue)
                        yield return (tagType, tag, value);
                    break;
            }
        }
    }
}
