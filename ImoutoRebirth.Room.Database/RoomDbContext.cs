using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using ImoutoRebirth.Room.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.Database
{
    public class RoomDbContext : DbContext
    {

        public DbSet<CollectionEntity> Collections { get; set; } = default!;

        public DbSet<SourceFolderEntity> SourceFolders { get; set; } = default!;

        public DbSet<DestinationFolderEntity> DestinationFolders { get; set; } = default!;

        public DbSet<CollectionFileEntity> CollectionFiles { get; set; } = default!;

        public RoomDbContext(DbContextOptions options)
            : base(options)
        {
        }

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
}