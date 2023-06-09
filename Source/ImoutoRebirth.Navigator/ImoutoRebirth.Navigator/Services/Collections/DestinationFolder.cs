﻿namespace ImoutoRebirth.Navigator.Services.Collections;

public class DestinationFolder
{
    public Guid? Id { get; }

    public Guid CollectionId { get; }

    public string Path { get; }

    public bool ShouldCreateSubfoldersByHash { get; }

    public bool ShouldRenameByHash { get; }

    public string FormatErrorSubfolder { get; }

    public string HashErrorSubfolder { get; }

    public string WithoutHashErrorSubfolder { get; }

    public DestinationFolder(
        Guid? id,
        Guid collectionId,
        string path,
        bool shouldCreateSubfoldersByHash,
        bool shouldRenameByHash,
        string formatErrorSubfolder,
        string hashErrorSubfolder,
        string withoutHashErrorSubfolder)
    {
        Id = id;
        CollectionId = collectionId;
        Path = path;
        ShouldCreateSubfoldersByHash = shouldCreateSubfoldersByHash;
        ShouldRenameByHash = shouldRenameByHash;
        FormatErrorSubfolder = formatErrorSubfolder;
        HashErrorSubfolder = hashErrorSubfolder;
        WithoutHashErrorSubfolder = withoutHashErrorSubfolder;
    }
}