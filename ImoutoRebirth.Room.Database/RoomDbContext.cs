using System.Threading;
using System.Threading.Tasks;
using EFSecondLevelCache.Core;
using EFSecondLevelCache.Core.Contracts;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using ImoutoRebirth.Room.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ImoutoRebirth.Room.Database
{
    public class RoomDbContext : DbContext
    {

        public DbSet<CollectionEntity> Collections { get; set; }

        public DbSet<SourceFolderEntity> SourceFolders { get; set; }

        public DbSet<DestinationFolderEntity> DestinationFolders { get; set; }

        public DbSet<CollectionFileEntity> CollectionFiles { get; set; }

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

            var changedEntityNames = GetChangedNames();

            var result = base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            InvalidateSecondLevelCache(changedEntityNames);

            return result;
        }

        private void InvalidateSecondLevelCache(string[] changedEntityNames)
        {
            this.GetService<IEFCacheServiceProvider>().InvalidateCacheDependencies(changedEntityNames);
        }

        private string[] GetChangedNames()
        {
            ChangeTracker.DetectChanges();
            return this.GetChangedEntityNames();
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
                        .Property<string>("SupportedExtensions")
                        // rename to _supportedExtensions after ef core 3.0 release
                        .HasField("SupportedExtensions");
        }

        private static void BuildCollectionFileEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CollectionFileEntity>()
                        .HasIndex(entity => entity.Path);
        }
    }
}