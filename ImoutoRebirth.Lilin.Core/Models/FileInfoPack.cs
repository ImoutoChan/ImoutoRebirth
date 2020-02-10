using System.Collections.Generic;
using System.Linq;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Core.Models
{
    public class FileInfoPack
    {
        public IReadOnlyCollection<FileInfo> Files { get; }
        
        public FileInfoPack(IReadOnlyCollection<FileInfo> files)
        {
            Files = files;
        }

        public void UpdateTags(IReadOnlyCollection<FileTag> newFileTags)
        {
            foreach (var file in Files)
            {
                foreach (var newTag in newFileTags.Where(x => x.FileId == file.Id))
                {
                    var existingTags = file.Tags.Where(x => x.IsSameIdentity(newTag));

                    var shouldSkip = existingTags.Any(x => x.Value == newTag.Value);

                    if (shouldSkip)
                        continue;

                    file.AddFileTag(newTag);
                }
            }
        }
    }
}