namespace ImoutoRebirth.Navigator.Services.Tags
{
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
}