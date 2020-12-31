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
        public IReadOnlyCollection<string> GetTagsFromName(FileInfo fileInfo) 
            => new[] {fileInfo.Name};

        public IReadOnlyCollection<string> GetTagsFromPath(SourceFolder sourceDirectory, FileInfo fileInfo) 
            => GetTags(sourceDirectory, fileInfo);

        private IReadOnlyCollection<string> GetTags(SourceFolder sourceDirectory, FileInfo fileInfo)
        {
            var sourcePathEntries = GetPathParts(new DirectoryInfo(sourceDirectory.Path));
            var filePathEntries = GetPathParts(fileInfo);

            return filePathEntries.Except(sourcePathEntries).ToArray();
        }

        private static IEnumerable<string> GetPathParts(DirectoryInfo directoryInfo)
        {
            var directory = directoryInfo;

            while(directory != null)
            {
                yield return directory.Name;
                directory = directory.Parent;
            }
        }

        private static IEnumerable<string> GetPathParts(FileInfo fileInfo)
        {
            foreach (var directoryPart in GetPathParts(fileInfo.Directory))
            {
                yield return directoryPart;
            }
        }
    }
}