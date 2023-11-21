namespace ImoutoRebirth.Navigator.Services.Collections;

public record DestinationFolder(
    Guid? Id,
    Guid CollectionId,
    string Path,
    bool ShouldCreateSubfoldersByHash,
    bool ShouldRenameByHash,
    string FormatErrorSubfolder,
    string HashErrorSubfolder,
    string WithoutHashErrorSubfolder);
