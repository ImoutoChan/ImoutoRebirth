using System;
using System.Collections.Generic;

namespace ImoutoRebirth.Navigator.Services.Collections
{
    public class SourceFolder
    {
        public Guid? Id { get; }

        public Guid CollectionId { get; }

        public string Path { get; }

        public bool ShouldCheckFormat { get; }

        public bool ShouldCheckHashFromName { get; }

        public bool ShouldCreateTagsFromSubfolders { get; }

        public bool ShouldAddTagFromFilename { get; }

        public IReadOnlyCollection<string> SupportedExtensions { get; }

        public SourceFolder(
            Guid? id,
            Guid collectionId,
            string path,
            bool shouldCheckFormat,
            bool shouldCheckHashFromName,
            bool shouldCreateTagsFromSubfolders,
            bool shouldAddTagFromFilename,
            IReadOnlyCollection<string> supportedExtensions)
        {
            Id = id;
            CollectionId = collectionId;
            Path = path;
            ShouldCheckFormat = shouldCheckFormat;
            ShouldCheckHashFromName = shouldCheckHashFromName;
            ShouldCreateTagsFromSubfolders = shouldCreateTagsFromSubfolders;
            ShouldAddTagFromFilename = shouldAddTagFromFilename;
            SupportedExtensions = supportedExtensions;
        }
    }
}