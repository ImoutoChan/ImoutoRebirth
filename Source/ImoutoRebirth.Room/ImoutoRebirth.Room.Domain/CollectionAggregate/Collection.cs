using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Room.Domain.CollectionAggregate;

public class Collection
{
    public Collection(
        Guid id,
        string name,
        DestinationFolder destinationFolder,
        IReadOnlyCollection<SourceFolder> sourceFolders)
    {
        Id = id;
        Name = name;
        DestinationFolder = destinationFolder;
        SourceFolders = sourceFolders;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public DestinationFolder DestinationFolder { get; private set; }

    public IReadOnlyCollection<SourceFolder> SourceFolders { get; private set; }
    
    public static Collection Create(string name)
    {
        return new(
            Guid.NewGuid(),
            name,
            DestinationFolder.Default,
            Array.Empty<SourceFolder>());
    }

    public void Rename(string newName) => Name = newName;

    public Guid SetDestinationFolder(
        string path,
        bool shouldCreateSubfoldersByHash,
        bool shouldRenameByHash,
        string formatErrorSubfolder,
        string hashErrorSubfolder,
        string withoutHashErrorSubfolder)
    {
        var newId = Guid.NewGuid();
        DestinationFolder = new DestinationFolder(
            newId,
            path,
            shouldCreateSubfoldersByHash,
            shouldRenameByHash,
            formatErrorSubfolder,
            hashErrorSubfolder,
            withoutHashErrorSubfolder);

        return newId;
    }

    public void DeleteDestinationFolder()
    {
        DestinationFolder = DestinationFolder.Default;
    }

    public DomainResult<Guid> AddSourceFolder(
        string path,
        bool shouldCheckFormat,
        bool shouldCheckHashFromName,
        bool shouldCreateTagsFromSubfolders,
        bool shouldAddTagFromFilename,
        IReadOnlyCollection<string> supportedExtensions,
        bool isWebhookUploadEnabled,
        string? webhookUploadUrl)
    {
        var id = Guid.NewGuid();
        var sourceFolder = new SourceFolder(
            id,
            path,
            shouldCheckFormat,
            shouldCheckHashFromName,
            shouldCreateTagsFromSubfolders,
            shouldAddTagFromFilename,
            supportedExtensions,
            isWebhookUploadEnabled,
            webhookUploadUrl);
        
        SourceFolders = SourceFolders.Append(sourceFolder).ToList();

        return new DomainResult<Guid>(id)
        {
            new SourceFoldersUpdatedDomainEvent()
        };
    }

    public DomainResult RemoveSourceFolder(Guid id)
    {
        SourceFolders = SourceFolders.Where(x => x.Id != id).ToList();

        return [new SourceFoldersUpdatedDomainEvent()];
    }

    public DomainResult UpdateSourceFolder(
        Guid sourceFolderId,
        string path,
        bool shouldCheckFormat,
        bool shouldCheckHashFromName,
        bool shouldCreateTagsFromSubfolders,
        bool shouldAddTagFromFilename,
        IReadOnlyCollection<string> supportedExtensions,
        bool isWebhookUploadEnabled,
        string? webhookUploadUrl)
    {
        var sourceFolder = SourceFolders.FirstOrDefault(x => x.Id == sourceFolderId);
        if (sourceFolder == null)
            throw new ArgumentException($"Source folder with id {sourceFolderId} not found");

        sourceFolder = new SourceFolder(
            sourceFolderId,
            path,
            shouldCheckFormat,
            shouldCheckHashFromName,
            shouldCreateTagsFromSubfolders,
            shouldAddTagFromFilename,
            supportedExtensions,
            isWebhookUploadEnabled,
            webhookUploadUrl);
        
        SourceFolders = SourceFolders.Where(x => x.Id != sourceFolderId).Append(sourceFolder).ToList();

        return [new SourceFoldersUpdatedDomainEvent()];
    }
}

public record SourceFoldersUpdatedDomainEvent : IDomainEvent;
