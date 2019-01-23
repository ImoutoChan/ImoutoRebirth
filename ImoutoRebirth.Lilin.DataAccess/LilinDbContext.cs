using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.DataAccess
{
    public class LilinDbContext : DbContext, IUnitOfWork
    {
        private readonly SoftDeleteDbContextHelper<EntityBase> _softDeleteDbContextHelper;
        private readonly TimeTrackDbContextHelper _timeTrackDbContextHelper;

        public DbSet<TagTypeEntity> TagTypes { get; set; }

        public DbSet<TagEntity> Tags { get; set; }

        public DbSet<NoteEntity> Notes { get; set; }

        public DbSet<FileTagEntity> FileTags { get; set; }

        public LilinDbContext(
            DbContextOptions<LilinDbContext> options,
            SoftDeleteDbContextHelper<EntityBase> softDeleteDbContextHelper,
            TimeTrackDbContextHelper timeTrackDbContextHelper) 
            : base(options)
        {
            _softDeleteDbContextHelper = softDeleteDbContextHelper;
            _timeTrackDbContextHelper = timeTrackDbContextHelper;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileTagEntity>(b =>
            {
                b.HasKey(x => new {x.FileId, x.TagId, x.Source});
                b.HasOne(x => x.Tag)
                 .WithMany(x => x.FileTags)
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(x => new {x.FileId});
                b.HasIndex(x => new {x.TagId});
                b.HasIndex(x => new {x.TagId, x.Value});
                b.HasIndex(x => new {x.Source, x.TagId});
            });


            modelBuilder.Entity<TagEntity>(b =>
            {
                b.HasOne(x => x.Type)
                 .WithMany(x => x.Tags)
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(x => new {x.TypeId, x.Name});
                b.HasIndex(x => new {x.Name});
                b.Property(x => x.Count).HasDefaultValue(0);
            });

            modelBuilder.Entity<NoteEntity>(b =>
            {
                b.HasIndex(x => x.FileId);
            });

            modelBuilder.Entity<TagTypeEntity>(b =>
            {
                b.HasIndex(x => x.Name).IsUnique();
            });

            modelBuilder.Entity<TagEntity>(b =>
            {
                b.HasIndex(x => new {x.TypeId, x.Name}).IsUnique();
            });

            _softDeleteDbContextHelper.OnModelCreating(modelBuilder);
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            _softDeleteDbContextHelper.OnBeforeSaveChanges(ChangeTracker);
            _timeTrackDbContextHelper.OnBeforeSaveChanges(ChangeTracker);

            return base.SaveChangesAsync(cancellationToken);
        }

        public async Task<IDisposable> CreateTransaction(IsolationLevel isolationLevel)
        {
            return await Database.BeginTransactionAsync(isolationLevel);
        }

        public void CommitTransaction()
        {
            var currentTransaction = Database.CurrentTransaction;

            if (currentTransaction == null)
                throw new Exception("Can't commit empty transaction.");

            currentTransaction.Commit();
        }
    }
}
