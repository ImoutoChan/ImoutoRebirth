﻿namespace ImoutoViewer.ImoutoRebirth.Services.Collections;

public record SourceFolder(
    Guid? Id,
    Guid CollectionId,
    string Path,
    bool ShouldCheckFormat,
    bool ShouldCheckHashFromName,
    bool ShouldCreateTagsFromSubfolders,
    bool ShouldAddTagFromFilename,
    IReadOnlyCollection<string> SupportedExtensions);
