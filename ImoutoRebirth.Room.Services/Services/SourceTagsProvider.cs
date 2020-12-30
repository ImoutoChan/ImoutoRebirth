using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.Core.Services
{
    internal class SourceTagsProvider : ISourceTagsProvider
    {
        public IReadOnlyCollection<string> GetTags(SourceFolder sourceDirectory, FileInfo fileInfo)
        {
            static string[] GetPathParts(string path)
                => path.Split(
                    new[]
                    {
                        Path.VolumeSeparatorChar,
                        Path.AltDirectorySeparatorChar,
                        Path.DirectorySeparatorChar,
                        Path.PathSeparator
                    },
                    StringSplitOptions.RemoveEmptyEntries);

            var directory = fileInfo.Directory;
            if (directory == null)
                return Array.Empty<string>();

            var sourcePathEntries = GetPathParts(sourceDirectory.Path);
            var filePathEntries = GetPathParts(directory.FullName);

            return filePathEntries.Except(sourcePathEntries).ToArray();
        }
    }
}