using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Lilin.Domain.FileInfoAggregate;

public class FileInfo
{
    private List<FileTag> _tags;
    private List<FileNote> _notes;

    public Guid FileId { get; }

    public IReadOnlyCollection<FileTag> Tags => _tags;

    public IReadOnlyCollection<FileNote> Notes => _notes;

    public FileInfo(IEnumerable<FileTag> tags, IEnumerable<FileNote> notes, Guid fileId)
    {
        FileId = fileId;
        _tags = new List<FileTag>(tags);
        _notes = new List<FileNote>(notes);
    }

    public DomainResult UpdateMetadata(
        MetadataSource source,
        IReadOnlyCollection<FileTag> newFileTags,
        IReadOnlyCollection<FileNote> newFileNotes)
    {
        if (source != MetadataSource.Manual)
        {
            _tags = _tags.Where(x => x.Source != source).ToList();
            _notes = _notes.Where(x => x.Source != source).ToList();
        }

        foreach (var tag in newFileTags)
        {
            if (_tags.Any(x => x.Equals(tag)))
                continue;

            _tags.Add(tag);
        }

        foreach (var note in newFileNotes)
        {
            _notes = _notes.Where(x => !x.IsSameIdentity(note)).ToList();
            _notes.Add(note);
        }

        return new DomainResult { new FileInfoUpdatedDomainEvent(FileId, source) };
    }

    public void RemoveFileTag(Guid tagId, MetadataSource source, string? value)
    {
        var tag = Tags.FirstOrDefault(
            x => x.TagId == tagId
                 && x.Source == source
                 && value == x.Value);

        if (tag != null)
            _tags.Remove(tag);
    }

    public void UpdateTags(IReadOnlyCollection<FileTag> fileTags, SameTagHandleStrategy sameTagHandleStrategy)
    {
        foreach (var newTag in fileTags)
        {
            UpdateTag(newTag, sameTagHandleStrategy);
        }
    }

    private void UpdateTag(FileTag newTag, SameTagHandleStrategy sameTagHandleStrategy)
    {
        var containsSameTagWithSameValue = Tags.Any(x => x.Equals(newTag));
        if (containsSameTagWithSameValue)
            return;

        var sameTagWithDifferentValue = Tags.FirstOrDefault(x => x.IsSameIdentity(newTag));

        if (sameTagWithDifferentValue != null)
        {
            switch (sameTagHandleStrategy)
            {
                case SameTagHandleStrategy.ReplaceExistingValue:
                    sameTagWithDifferentValue.UpdateValue(newTag.Value);
                    break;
                case SameTagHandleStrategy.AddNewFileTag:
                    _tags.Add(newTag);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(sameTagHandleStrategy),
                        sameTagHandleStrategy,
                        null);
            }
        }
        else
        {
            _tags.Add(newTag);
        }
    }
}
