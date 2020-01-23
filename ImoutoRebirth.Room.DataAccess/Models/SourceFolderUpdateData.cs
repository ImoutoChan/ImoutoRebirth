using System;
using System.Collections.Generic;

namespace ImoutoRebirth.Room.DataAccess.Models
{
    public class SourceFolderUpdateData
    {
        public Guid Id { get; }

        public string Path { get; }

        public bool ShouldCheckFormat { get; }

        public bool ShouldCheckHashFromName { get; }

        public bool ShouldCreateTagsFromSubfolders { get; }

        public bool ShouldAddTagFromFilename { get; }

        public IReadOnlyCollection<string> SupportedExtensions { get; }

        public SourceFolderUpdateData(
            Guid id,
            string path,
            bool shouldCheckFormat,
            bool shouldCheckHashFromName,
            bool shouldCreateTagsFromSubfolders,
            bool shouldAddTagFromFilename,
            IReadOnlyCollection<string> supportedExtensions)
        {
            Id = id;
            Path = path;
            ShouldCheckFormat = shouldCheckFormat;
            ShouldCheckHashFromName = shouldCheckHashFromName;
            ShouldCreateTagsFromSubfolders = shouldCreateTagsFromSubfolders;
            ShouldAddTagFromFilename = shouldAddTagFromFilename;
            SupportedExtensions = supportedExtensions;
        }
    }
}