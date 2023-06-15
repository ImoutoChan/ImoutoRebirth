using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImoutoRebirth.Lilin.DataAccess.Entities;

public class TagEntity : EntityBase
{
    private const string SynonymsSeparator = ":.:";

    public Guid TypeId { get; set; }

    [Required]
    public required string Name { get; set; }

    public bool HasValue { get; set; }

    /// <summary>
    /// Values separator: :.: -- SynonymsSeparator
    /// </summary>
    public string? Synonyms { get; set; }

    public int Count { get; set; }

    [NotMapped]
    public IReadOnlyCollection<string> SynonymsArray
    {
        get => Synonyms?.Split(new [] {SynonymsSeparator}, StringSplitOptions.RemoveEmptyEntries) 
               ?? Array.Empty<string>();

        set => Synonyms = value.Any() 
            ? string.Join(SynonymsSeparator, value) 
            : null;
    }


    public TagTypeEntity? Type { get; set; }

    public IReadOnlyCollection<FileTagEntity>? FileTags { get; set; }
}
