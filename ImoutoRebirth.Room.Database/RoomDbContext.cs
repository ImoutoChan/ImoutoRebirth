using System.Threading;
using System.Threading.Tasks;
using EFSecondLevelCache.Core;
using EFSecondLevelCache.Core.Contracts;
using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using ImoutoRebirth.Room.Database.Entities;
using ImoutoRebirth.Room.Database.Entities.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ImoutoRebirth.Room.Database
{
    public class RoomDbContext : DbContext
    {
        private readonly SoftDeleteDbContextHelper<EntityBase> _softDeleteDbContextHelper;
        private readonly TimeTrackDbContextHelper _timeTrackDbContextHelper;

        public DbSet<CollectionEntity> Collections { get; set; }

        public DbSet<SourceFolderEntity> SourceFolders { get; set; }

        public DbSet<DestinationFolderEntity> DestinationFolders { get; set; }

        public DbSet<CollectionFileEntity> CollectionFiles { get; set; }

        public RoomDbContext(DbContextOptions options, SoftDeleteDbContextHelper<EntityBase> softDeleteDbContextHelper, TimeTrackDbContextHelper timeTrackDbContextHelper)
            : base(options)
        {
            _softDeleteDbContextHelper = softDeleteDbContextHelper;
            _timeTrackDbContextHelper = timeTrackDbContextHelper;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BuildSourceFolderEntity(modelBuilder);
            BuildDestinationFolderEntity(modelBuilder);
            BuildCollectionFileEntity(modelBuilder);

            _softDeleteDbContextHelper.OnModelCreating(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken())
        {
            _softDeleteDbContextHelper.OnBeforeSaveChanges(ChangeTracker);
            _timeTrackDbContextHelper.OnBeforeSaveChanges(ChangeTracker);

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
               .Property(x => x.FormatErrorSubfolder);

            modelBuilder
               .Entity<DestinationFolderEntity>()
               .Property(x => x.HashErrorSubfolder);

            modelBuilder
               .Entity<DestinationFolderEntity>()
               .Property(x => x.WithoutHashErrorSubfolder);
        }

        private static void BuildSourceFolderEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SourceFolderEntity>()
                        .Property<string>("SupportedExtensions")
                        .HasField("_supportedExtensions");
        }

        private static void BuildCollectionFileEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CollectionFileEntity>()
                        .HasIndex(entity => entity.Path);
        }
    }
}