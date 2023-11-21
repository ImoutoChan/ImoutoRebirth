using System.Data;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Common.Domain.EntityFrameworkCore;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using ImoutoRebirth.Room.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.Database;

public class RoomDbContext : DbContext, IUnitOfWork
{
    private readonly IEventStorage _eventStorage;

    public RoomDbContext(DbContextOptions options, IEventStorage eventStorage) : base(options)
    {
        _eventStorage = eventStorage;
    }

    public required DbSet<CollectionEntity> Collections { get; set; }

    public required DbSet<SourceFolderEntity> SourceFolders { get; set; }

    public required DbSet<DestinationFolderEntity> DestinationFolders { get; set; }

    public required DbSet<CollectionFileEntity> CollectionFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        BuildSourceFolderEntity(modelBuilder);
        BuildDestinationFolderEntity(modelBuilder);
        BuildCollectionFileEntity(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    public async Task SaveEntitiesAsync(CancellationToken ct = default)
    {
        await SaveChangesAsync(ct);

        foreach (var domainEvent in ChangeTracker
                     .Entries()
                     .Select(x => x.Entity)
                     .OfType<Entity>()
                     .SelectMany(x => x.Events))
        {
            _eventStorage.Add(domainEvent);
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ChangeTracker.TrackImplicitTimeBeforeSaveChanges();

        return base.SaveChangesAsync(ct);
    }

    public async Task<ITransaction> CreateTransactionAsync(IsolationLevel isolationLevel)
    {
        if (Database.CurrentTransaction != null)
            return new EmptyTransaction();

        return new DbTransaction(await Database.BeginTransactionAsync(isolationLevel));
    }

    private static void BuildDestinationFolderEntity(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<DestinationFolderEntity>()
            .Property(x => x.FormatErrorSubfolder)
            .HasDefaultValue("!FormatError");

        modelBuilder
            .Entity<DestinationFolderEntity>()
            .Property(x => x.HashErrorSubfolder)
            .HasDefaultValue("!HashError");

        modelBuilder
            .Entity<DestinationFolderEntity>()
            .Property(x => x.WithoutHashErrorSubfolder)
            .HasDefaultValue("!WithoutHashError");
    }

    private static void BuildSourceFolderEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SourceFolderEntity>()
            .Property<string>("_supportedExtensions")
            .HasColumnName("SupportedExtensions");
    }

    private static void BuildCollectionFileEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CollectionFileEntity>()
            .HasIndex(entity => entity.Path);

        modelBuilder.Entity<CollectionFileEntity>()
            .HasIndex(entity => entity.Md5);

        modelBuilder.Entity<CollectionFileEntity>()
            .HasIndex(entity => entity.IsRemoved);
            
        modelBuilder.Entity<CollectionFileEntity>()
            .HasIndex(entity => new
            {
                entity.CollectionId, 
                entity.Md5, 
                entity.IsRemoved
            })
            .HasFilter($"NOT \"{nameof(CollectionFileEntity.IsRemoved)}\"")
            .IsUnique();

        // Maybe add IsRemoved + Fields indexes

        modelBuilder.Entity<CollectionFileEntity>()
            .HasQueryFilter(x => !x.IsRemoved);
    }
}
