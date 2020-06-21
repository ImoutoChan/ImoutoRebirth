using System;

namespace Imouto.Viewer.ImoutoRebirthNavigator
{
    internal class SearchTagDto
    {
        public Guid TagId { get; }

        public string Value { get; }

        public SearchType SearchType { get; }

        public SearchTagDto(Guid tagId, string value, SearchType searchType)
        {
            TagId = tagId;
            Value = value;
            SearchType = searchType;
        }
    }
    internal enum SearchType
    {
        Include = 0,
        Exclude = 1
    }
}