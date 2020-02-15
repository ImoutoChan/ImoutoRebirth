#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImoutoRebirth.Navigator.Services.Tags
{
    interface IFileService
    {
        Task<IReadOnlyCollection<File>> SearchFiles(
            Guid? collectionId, 
            IReadOnlyCollection<SearchTag> tags, 
            int take, 
            int skip);

        Task<int> CountFiles(
            Guid? collectionId, 
            IReadOnlyCollection<SearchTag> tags);

        Task RemoveFile(Guid fileId);
    }

    public class File
    {
        public string Path { get; }

        public Guid Id { get; }
    }

    interface ITagService
    {
        Task<IReadOnlyCollection<TagType>> GеtTypes();

        Task CreateTag(TagType type, string name, bool hasValue, IReadOnlyCollection<string> synonyms);

        Task<IReadOnlyCollection<Tag>> SearchTags(string name, int count);
    }

    internal class Tag
    {
        public Guid? Id { get; }

        public string Title { get; }

        public TagType Type { get; }

        public IReadOnlyCollection<string> SynonymsCollection { get; }

        public bool HasValue { get; }
    }

    internal class TagType
    {
        public string Title { get; }

        public int Color { get; }
    }

    interface IFileTagService
    {
        Task SetRate(Guid fileId, Rate rate);

        Task SetFavorite(Guid fileId, bool value);

        Task BindTags(IReadOnlyCollection<FileTag> fileTags);

        Task UnbindTag(Guid fileId, Guid tagId);

        Task<IReadOnlyCollection<FileTag>> GetFileTags(Guid fileId);
    }

    internal class FileTag
    {
        public FileTag(Guid fileId, Tag tag, string? value, FileTagSource source)
        {
            FileId = fileId;
            Value = value;
            Source = source;
            Tag = tag;
        }

        public Guid FileId { get; }

        public Tag Tag { get; }

        public string? Value { get; set; }

        public FileTagSource Source { get; }

        public bool IsEditable => Source == FileTagSource.Manual;
    }

    internal class SearchTag
    {
        public SearchTag(Tag tag, string? value, SearchType searchType = SearchType.Include)
        {
            Tag = tag;
            Value = value;
            SearchType = searchType;
        }

        public Tag Tag { get; }

        public string? Value { get; }

        public SearchType SearchType { get; set; }
    }

    internal enum FileTagSource
    {
        Yandere = 0,
        Danbooru = 1,
        Sankaku = 2,
        Manual = 3
    }

    internal enum SearchType
    {
        Include = 0,
        Exclude = 1
    }

    public struct Rate
    {
        public int Rating { get; }

        public Rate(int value)
        {
            if (value < 0 || value > 5)
                throw new ArgumentOutOfRangeException(nameof(value));

            Rating = value;
        }
    }
}
