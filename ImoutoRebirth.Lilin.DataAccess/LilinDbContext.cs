using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.DataAccess
{
    public class LilinDbContext : DbContext, IUnitOfWork
    {
        private readonly IEventStorage _eventStorage;

        public DbSet<TagTypeEntity> TagTypes { get; set; } = default!;

        public DbSet<TagEntity> Tags { get; set; } = default!;

        public DbSet<NoteEntity> Notes { get; set; } = default!;

        public DbSet<FileTagEntity> FileTags { get; set; } = default!;

        public LilinDbContext(
            DbContextOptions<LilinDbContext> options,
            IEventStorage eventStorage) 
            : base(options)
        {
            _eventStorage = eventStorage;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileTagEntity>(b =>
            {
                b.HasOne(x => x.Tag)
                 .WithMany(x => x!.FileTags)
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Restrict);
                
                b.HasIndex(x => new {x.FileId});
                b.HasIndex(x => new {x.TagId});
                b.HasIndex(x => new {x.TagId, x.Value});
                b.HasIndex(x => new {x.Source, x.TagId});
                b.HasIndex(x => new {x.FileId, x.TagId, x.Source});
            });


            modelBuilder.Entity<TagEntity>(b =>
            {
                b.HasOne(x => x.Type)
                 .WithMany(x => x!.Tags)
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(x => new {x.Name});
                b.Property(x => x.Count).HasDefaultValue(0);

                b.HasIndex(x => new { x.TypeId, x.Name }).IsUnique();
            });

            modelBuilder.Entity<NoteEntity>(b =>
            {
                b.HasIndex(x => x.FileId);
            });

            modelBuilder.Entity<TagTypeEntity>(b =>
            {
                b.HasIndex(x => x.Name).IsUnique();
            });
        }

        public async Task SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            await SaveChangesAsync(cancellationToken);

            foreach (var domainEvent in ChangeTracker
                                       .Entries()
                                       .Select(x => x.Entity)
                                       .OfType<Entity>()
                                       .SelectMany(x => x.Events))
            {
                _eventStorage.Add(domainEvent);
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ChangeTracker.TrackImplicitTimeBeforeSaveChanges();

            return base.SaveChangesAsync(cancellationToken);
        }

        public async Task<ITransaction> CreateTransactionAsync(IsolationLevel isolationLevel)
        {
            if (Database.CurrentTransaction != null)
                return new EmptyTransaction();

            return new DbTransaction(await Database.BeginTransactionAsync(isolationLevel));
        }
    }
}
