using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.Models;
using ImoutoRebirth.Common;
using Note = ImoutoRebirth.Arachne.Core.Models.Note;
using Post = Imouto.BooruParser.Post;
using SearchResult = ImoutoRebirth.Arachne.Core.Models.SearchResult;
using Tag = ImoutoRebirth.Arachne.Core.Models.Tag;

namespace ImoutoRebirth.Arachne.Infrastructure;

internal class BooruPostConverter : IBooruPostConverter
{
    public SearchResult Convert(
        Post post, 
        Image image, 
        SearchEngineType searchEngineType)
    {
        var metadataParsingDto = ToMetadataParsingResults(post);

        var metaTags = metadataParsingDto
            .GetMetaTags()
            .Select(x => new Tag(x.TagType, x.Tag, x.Value));
        var postTags = metadataParsingDto
            .Tags
            .Select(x => new Tag(x.TagType, x.Tag, x.Value, x.Synonyms));

        return new Metadata(
            image, 
            searchEngineType, 
            true, 
            metaTags.Union(postTags).ToArray(), 
            metadataParsingDto.Notes,
            post.Id.Id);
    }

    private static MetaParsingResults ToMetadataParsingResults(Post post)
    {
        var uploaderId = post.UploaderId.Id > 0 ? post.UploaderId.Id.ToString() : null;
        var uploaderName = !string.IsNullOrWhiteSpace(post.UploaderId.Name) ? post.UploaderId.Id.ToString() : null;

        return new MetaParsingResults(
            post.Source!,
            post.Id.Id.ToString(),
            DateTimeOffset.UtcNow.ToString(Constants.DateTimeFormat),
            post.Id.Md5Hash,
            post.PostedAt.ToString(Constants.DateTimeFormat),
            uploaderId,
            uploaderName,
            post.Rating.ToString(),
            post.RatingSafeLevel.ToString(),
            post.Parent?.Id > 0 ? post.Parent.Id.ToString() : null,
            post.Parent != null && !string.IsNullOrWhiteSpace(post.Parent.Md5Hash) ? post.Parent.Md5Hash : null,
            post.ChildrenIds.Select(x => $"{x.Id}:{x.Md5Hash}").ToList(),
            post.Pools.Select(x => x.Id + "||" + x.Name + "|||" + x.Position).ToArray(),
            post.Tags.Where(x => x.Type.ToLower() != "deprecated").Select(ConvertTag).ToArray(),
            post.Notes.Select(ConvertNote).ToArray(),
            post.UgoiraFrameDelays.Any()
                ? new UgoiraFrameData(
                    "jpeg",
                    post.UgoiraFrameDelays.Select((x, i) => new FrameData(x, $"{i:000000}" + ".jpg")).ToList())
                : null);
    }

    private static (string?, string?) GetParentInfo(string postParentId)
    {
        var (parentId, parentMd5) = ((string?)null, (string?)null);

        if (!string.IsNullOrWhiteSpace(postParentId))
        {
            var parts = postParentId.Split(':');
            parentId = parts[0];

            if (parts.Length > 1)
                parentMd5 = parts[1];
        }

        return (parentId, parentMd5);
    }

    private static MetaParsingTagResults ConvertTag(Imouto.BooruParser.Tag tag) =>
        new(GetType(tag), tag.Name.ToLowerInvariant(), Array.Empty<string>(), null);

    private static string GetType(Imouto.BooruParser.Tag tag)
    {
        var allowed = new[]
        {
            "LocalMeta",
            "General",
            "Genre",
            "Faults",
            "Studio",
            "Meta",
            "Copyright",
            "Character",
            "Artist",
            "Circle",
            "Medium",
            "Location"
        };

        if (allowed.Contains(tag.Type))
            return tag.Type;

        var type = tag.Type;

        type = type[0].ToString().ToUpperInvariant() + type[1..].ToLower();

        if (allowed.Contains(type))
            return type;
        
        return "General";
    }

    private static Note ConvertNote(Imouto.BooruParser.Note note) 
        => new(
            note.Text, 
            new NotePosition(
                note.Point.Top, 
                note.Point.Left, 
                note.Size.Width, 
                note.Size.Height),
            note.Id);
}
