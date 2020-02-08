using System.Collections.Generic;
using ImoutoRebirth.Arachne.Core.Models;

namespace ImoutoRebirth.Arachne.Infrastructure.Models
{
    internal class MetaParsingResults
    {
        public string Source { get; }

        public string BooruPostId { get; }
        
        public string BooruLastUpdate { get; }

        public string Md5 { get; }

        public string PostedDateTime { get; }

        public string PostedById { get; }

        public string PostedByUsername { get; }

        public string Rating { get; }

        public string ParentId { get; }

        public string ParentMd5 { get; }

        public IReadOnlyCollection<string> Childs { get; }

        public IReadOnlyCollection<string> Pools { get; }

        public IReadOnlyCollection<(string TagType, string Tag, string[] Synonyms)> Tags { get; }

        public IReadOnlyCollection<Note> Notes { get; }

        public MetaParsingResults(
            string source,
            string booruPostId,
            string booruLastUpdate,
            string md5,
            string postedDateTime,
            string postedById,
            string postedByUsername,
            string rating,
            string parentId,
            string parentMd5,
            IReadOnlyCollection<string> childs,
            IReadOnlyCollection<string> pools,
            IReadOnlyCollection<(string TagType, string Tag, string[] Synonyms)> tags,
            IReadOnlyCollection<Note> notes)
        {
            Source = source;
            BooruPostId = booruPostId;
            BooruLastUpdate = booruLastUpdate;
            Md5 = md5;
            PostedDateTime = postedDateTime;
            PostedById = postedById;
            PostedByUsername = postedByUsername;
            Rating = rating;
            ParentId = parentId;
            ParentMd5 = parentMd5;
            Childs = childs;
            Pools = pools;
            Tags = tags;
            Notes = notes;
        }

        public IEnumerable<(string TagType, string Tag, string Value)> GetMetaTags()
        {
            const string tagType = "LocalMeta";

            var props = typeof(MetaParsingResults).GetProperties();
            foreach (var propertyInfo in props)
            {
                var tag = propertyInfo.Name;

                if (tag == nameof(Tags) || tag == nameof(Notes))
                    continue;

                switch (propertyInfo.GetValue(this))
                {
                    case string strValue:
                        yield return (tagType, tag, strValue);
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
}