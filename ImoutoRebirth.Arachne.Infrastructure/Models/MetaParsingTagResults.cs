namespace ImoutoRebirth.Arachne.Infrastructure.Models
{
    internal record MetaParsingTagResults(
        string TagType,
        string Tag,
        IReadOnlyCollection<string> Synonyms,
        string? Value);
}
