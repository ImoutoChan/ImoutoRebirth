using System;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags.Model
{
    internal class SearchTag
    {
        public SearchTag(Guid tagId, string? value, SearchType searchType = SearchType.Include)
        {
            TagId = tagId;
            Value = value;
            SearchType = searchType;
        }

        public Guid TagId { get; }

        public string? Value { get; set; }

        public SearchType SearchType { get; set; }
    }
}