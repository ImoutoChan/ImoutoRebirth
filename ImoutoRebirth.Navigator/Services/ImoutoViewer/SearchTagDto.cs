using System;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.ImoutoViewer
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
}