namespace ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

internal class NewTag
{
    public NewTag(
        string title, 
        TagType type, 
        IReadOnlyCollection<string> synonymsCollection, 
        bool hasValue, 
        int count)
    {
        Title = title;
        Type = type;
        SynonymsCollection = synonymsCollection;
        HasValue = hasValue;
        Count = count;
    }

    public string Title { get; }

    public TagType Type { get; }

    public IReadOnlyCollection<string> SynonymsCollection { get; }

    public bool HasValue { get; }

    public int Count { get; }
}