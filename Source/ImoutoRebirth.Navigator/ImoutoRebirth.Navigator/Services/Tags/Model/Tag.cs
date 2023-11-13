namespace ImoutoRebirth.Navigator.Services.Tags.Model;

internal class Tag
{
    public Tag(
        Guid id, 
        string title, 
        TagType type, 
        IReadOnlyCollection<string> synonymsCollection, 
        bool hasValue,
        bool isCounter,
        int count)
    {
        Id = id;
        Title = title;
        Type = type;
        SynonymsCollection = synonymsCollection;
        HasValue = hasValue;
        IsCounter = isCounter;
        Count = count;
    }

    public Guid Id { get; }

    public string Title { get; }

    public TagType Type { get; }

    public IReadOnlyCollection<string> SynonymsCollection { get; }

    public bool HasValue { get; }
    
    public bool IsCounter { get; }

    public int Count { get; }
}
