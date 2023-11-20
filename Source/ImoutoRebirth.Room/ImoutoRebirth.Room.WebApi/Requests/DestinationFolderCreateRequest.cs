using System.ComponentModel.DataAnnotations;

namespace ImoutoRebirth.Room.WebApi.Requests;

public class DestinationFolderCreateRequest
{
    [Required]
    public string Path { get; set; } = default!;

    public bool ShouldCreateSubfoldersByHash { get; set; }

    public bool ShouldRenameByHash { get; set; }

    [Required]
    public string FormatErrorSubfolder { get; set; } = default!;

    [Required]
    public string HashErrorSubfolder { get; set; } = default!;

    [Required]
    public string WithoutHashErrorSubfolder { get; set; } = default!;
}


public class SetDestinationFolderCreate
{
    [Required]
    public string Path { get; set; } = default!;

    public bool ShouldCreateSubfoldersByHash { get; set; }

    public bool ShouldRenameByHash { get; set; }

    [Required]
    public string FormatErrorSubfolder { get; set; } = default!;

    [Required]
    public string HashErrorSubfolder { get; set; } = default!;

    [Required]
    public string WithoutHashErrorSubfolder { get; set; } = default!;
}
