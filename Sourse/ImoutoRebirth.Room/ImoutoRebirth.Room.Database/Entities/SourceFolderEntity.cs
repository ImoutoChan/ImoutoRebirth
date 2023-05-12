using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImoutoRebirth.Room.Database.Entities.Abstract;

namespace ImoutoRebirth.Room.Database.Entities;

public class SourceFolderEntity : EntityBase
{
    private const string SupportedExtensionsSeparator = ";";

    private string? _supportedExtensions;

    [ForeignKey(nameof(Collection))]
    public Guid CollectionId { get; set; }

    [Required]
    public string Path { get; set; } = default!;

    public bool ShouldCheckFormat { get; set; }

    public bool ShouldCheckHashFromName { get; set; }

    public bool ShouldCreateTagsFromSubfolders { get; set; }

    public bool ShouldAddTagFromFilename { get; set; }

    [NotMapped]
    public IReadOnlyCollection<string>? SupportedExtensionCollection
    {
        get => _supportedExtensions?.Split(
            new[] {SupportedExtensionsSeparator}, 
            StringSplitOptions.RemoveEmptyEntries);
        set => _supportedExtensions = value != null 
            ? string.Join(SupportedExtensionsSeparator, value) 
            : null;
    }

    public CollectionEntity? Collection { get; set; }
}