using System;
using System.Linq;
using Imouto.BooruParser.Model.Base;
using ImoutoRebirth.Common;
using ImoutoRebirth.Shop.Core.Models;
using ImoutoRebirth.Shop.Infrastructure.Abstract;
using ImoutoRebirth.Shop.Infrastructure.Models;
using Note = ImoutoRebirth.Shop.Core.Models.Note;
using SearchResult = ImoutoRebirth.Shop.Core.Models.SearchResult;
using Tag = ImoutoRebirth.Shop.Core.Models.Tag;

namespace ImoutoRebirth.Shop.Infrastructure
{
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
                          .Select(x => new Tag(x.TagType, x.Tag, x.Value, Array.Empty<string>()));
            var postTags = metadataParsingDto
                          .Tags
                          .Select(x => new Tag(x.TagType, x.Tag, null, x.Synonyms));

            return new Metadata(
                image, 
                searchEngineType, 
                true, 
                metaTags.Union(postTags).ToArray(), 
                metadataParsingDto.Notes);
        }

        private static MetaParsingResults ToMetadataParsingResults(Post post)
        {
            var (parentId, parentMd5) = GetParentInfo(post.ParentId);
            
            return new MetaParsingResults(
                post.Source,
                post.PostId.ToString(),
                post.ActualDateTime.ToString(Constants.DateTimeFormat),
                post.Md5,
                post.PostedDateTime.ToLongTimeString(),
                post.PostedUser?.Id?.ToString(),
                post.PostedUser?.Name,
                post.ImageRating.ToString(),
                parentId,
                parentMd5,
                post.ChildrenIds,
                post.Pools.Select(x => x.Id + "||" + x.Name).ToArray(),
                post.Tags.Select(ConvertTag).ToArray(),
                post.Notes.Select(ConvertNote).ToArray());
        }

        private static (string, string) GetParentInfo(string postParentId)
        {
            var (parentId, parentMd5) = ((string)null, (string)null);

            if (!string.IsNullOrWhiteSpace(postParentId))
            {
                var parts = postParentId.Split(':');
                parentId = parts[0];

                if (parts.Length > 1)
                    parentMd5 = parts[1];
            }

            return (parentId, parentMd5);
        }

        private static (string, string, string[]) ConvertTag(Imouto.BooruParser.Model.Base.Tag tag)
            => (tag.Type.ToString(), tag.Name, new[] {tag.JapName});
        
        private static Note ConvertNote(Imouto.BooruParser.Model.Base.Note note) 
            => new Note(
                note.Label, 
                new NotePosition(
                    note.NotePoint.Top, 
                    note.NotePoint.Left, 
                    note.NoteSize.Width, 
                    note.NoteSize.Height));
    }
}