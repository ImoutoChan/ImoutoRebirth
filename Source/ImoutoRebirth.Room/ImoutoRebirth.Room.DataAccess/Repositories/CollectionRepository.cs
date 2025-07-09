using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.DataAccess.Exceptions;
using ImoutoRebirth.Room.DataAccess.Mappers;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Database.Entities;
using ImoutoRebirth.Room.Domain.CollectionAggregate;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.DataAccess.Repositories;

internal class CollectionRepository : ICollectionRepository
{
    private readonly RoomDbContext _roomDbContext;

    public CollectionRepository(RoomDbContext roomDbContext) => _roomDbContext = roomDbContext;

    public async Task<IReadOnlyCollection<Guid>> GetAllIds() 
        => await _roomDbContext.Collections.Select(x => x.Id).ToListAsync();

    public async Task<IReadOnlyCollection<Collection>> GetAll(CancellationToken ct)
    {
        var collections 
            = await _roomDbContext
                .Collections
                .AsNoTracking()
                .Include(x => x.DestinationFolder)
                .Include(x => x.SourceFolders)
                .ToListAsync(ct);
            
        return collections.Select(x => x.ToModel()).ToList();
    }

    public async Task<Collection?> GetById(Guid id)
    {
        var collection
            = await _roomDbContext
                .Collections
                .AsNoTracking()
                .Include(x => x.DestinationFolder)
                .Include(x => x.SourceFolders)
                .Where(x => x.Id == id)
                .SingleOrDefaultAsync();

        return collection?.ToModel();
    }

    public async Task Create(Collection collection)
    {
        var newCollectionEntity = collection.ToEntity();
        await _roomDbContext.Collections.AddAsync(newCollectionEntity);
        await _roomDbContext.SaveChangesAsync();
    }

    public async Task Update(Collection collection)
    {
        var collectionId = collection.Id;
        
        var collectionEntity = await _roomDbContext.Collections
            .Where(x => x.Id == collectionId)
            .SingleAsync();
            
        collectionEntity.Name = collection.Name;

        // update destination folder
        if (collection.DestinationFolder.IsDefault())
        {
            var destinationFolderEntity = await _roomDbContext.DestinationFolders
                .Where(x => x.CollectionId == collectionId)
                .FirstOrDefaultAsync();

            if (destinationFolderEntity != null) 
                _roomDbContext.Remove(destinationFolderEntity);
        }
        else
        {
            var destinationFolderEntity = await _roomDbContext.DestinationFolders
                .Where(x => x.CollectionId == collectionId)
                .SingleOrDefaultAsync();

            if (destinationFolderEntity != null)
            {
                destinationFolderEntity.Path = collection.DestinationFolder.DestinationDirectory!.FullName;
                destinationFolderEntity.ShouldCreateSubfoldersByHash = collection.DestinationFolder.ShouldCreateSubfoldersByHash;
                destinationFolderEntity.ShouldRenameByHash = collection.DestinationFolder.ShouldRenameByHash;
                destinationFolderEntity.FormatErrorSubfolder = collection.DestinationFolder.FormatErrorSubfolder;
                destinationFolderEntity.HashErrorSubfolder = collection.DestinationFolder.HashErrorSubfolder;
                destinationFolderEntity.WithoutHashErrorSubfolder = collection.DestinationFolder.WithoutHashErrorSubfolder;
            }
            else
            {
                await _roomDbContext.DestinationFolders.AddAsync(collection.DestinationFolder.ToEntity(collectionId)!);
            }
        }
        
        // update source folders
        var sourceFolderEntities = await _roomDbContext.SourceFolders
            .Where(x => x.CollectionId == collectionId)
            .ToListAsync();

        foreach (var collectionSourceFolder in collection.SourceFolders)
        {
            var sourceFolderEntity = sourceFolderEntities
                .SingleOrDefault(x => x.Id == collectionSourceFolder.Id);

            if (sourceFolderEntity == null)
            {
                sourceFolderEntity = collectionSourceFolder.ToEntity(collectionId);
                await _roomDbContext.SourceFolders.AddAsync(sourceFolderEntity);
            }
            else
            {
                sourceFolderEntity.Path = collectionSourceFolder.Path;
                sourceFolderEntity.ShouldCheckFormat = collectionSourceFolder.ShouldCheckFormat;
                sourceFolderEntity.ShouldCheckHashFromName = collectionSourceFolder.ShouldCheckHashFromName;
                sourceFolderEntity.ShouldCreateTagsFromSubfolders = collectionSourceFolder.ShouldCreateTagsFromSubfolders;
                sourceFolderEntity.ShouldAddTagFromFilename = collectionSourceFolder.ShouldAddTagFromFilename;
                sourceFolderEntity.SupportedExtensionCollection = collectionSourceFolder.SupportedExtensions;
                sourceFolderEntity.WebhookUploadUrl = collectionSourceFolder.WebhookUploadUrl;
                sourceFolderEntity.IsWebhookUploadEnabled = collectionSourceFolder.IsWebhookUploadEnabled;
            }
        }
        
        foreach (var sourceFolderEntity in sourceFolderEntities)
        {
            if (collection.SourceFolders.All(x => x.Id != sourceFolderEntity.Id))
                _roomDbContext.Remove(sourceFolderEntity);
        }

        await _roomDbContext.SaveChangesAsync();
    }

    public async Task Remove(Guid id)
    {
        var collection = await _roomDbContext.Collections.FirstOrDefaultAsync(x => x.Id == id);

        if (collection == null)
            throw new EntityNotFoundException<CollectionEntity>(id);

        _roomDbContext.Remove(collection);
        await _roomDbContext.SaveChangesAsync();
    }
}
