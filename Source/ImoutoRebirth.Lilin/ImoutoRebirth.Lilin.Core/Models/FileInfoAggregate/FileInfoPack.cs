namespace ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;

public class FileInfoPack
{
    public IReadOnlyCollection<FileInfo> Files { get; }
        
    public FileInfoPack(IReadOnlyCollection<FileInfo> files)
    {
        Files = files;
    }

    public void UpdateTags(IReadOnlyCollection<FileTag> newFileTags, SameTagHandleStrategy sameTagHandleStrategy)
    {
        foreach (var file in Files)
        {
            foreach (var newTag in newFileTags.Where(x => x.FileId == file.Id))
            {
                ProcessNewTagForFile(file, newTag, sameTagHandleStrategy);
            }
        }
    }

    private void ProcessNewTagForFile(FileInfo file, FileTag newTag, SameTagHandleStrategy sameTagHandleStrategy)
    {
        // file has same tag with exact tag + value
        var containsSameTag = file.Tags.Any(x => x.Equals(newTag));
        if (containsSameTag)
            return;


        var sameTagWithDifferentValue = file.Tags.FirstOrDefault(x => x.IsSameIdentity(newTag));
        if (sameTagWithDifferentValue == null)
        {
            // file doesn't have same tag
            file.AddFileTag(newTag);
            return;
        }

        // file has same tag but with other value
        switch (sameTagHandleStrategy)
        {
            case SameTagHandleStrategy.ReplaceExistingValue:
                sameTagWithDifferentValue.UpdateValue(newTag.Value);
                break;
            case SameTagHandleStrategy.AddNewFileTag:
                file.AddFileTag(newTag);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(sameTagHandleStrategy),
                    sameTagHandleStrategy,
                    null);
        }
    }
}