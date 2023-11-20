using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using ImoutoRebirth.Room.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.Database;

public class RoomDbContext(DbContextOptions options) : DbContext(options)
{
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

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new CancellationToken())
    {
        ChangeTracker.TrackImplicitTimeBeforeSaveChanges();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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
