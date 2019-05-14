using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ImoutoProject.Common.Cqrs.Behaviors;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using ImoutoRebirth.Meido.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Meido.DataAccess
{
    public class MeidoDbContext : DbContext, IUnitOfWork
    {
        private readonly TimeTrackDbContextHelper _timeTrackDbContextHelper;

        public DbSet<ParsingStatusEntity> ParsingStatuses { get; set; }

        public DbSet<SourceActualizingStateEntity> SourceActualizingStates { get; set; }


        public MeidoDbContext(
            DbContextOptions<MeidoDbContext> options,
            TimeTrackDbContextHelper timeTrackDbContextHelper)
            : base(options)
        {
            _timeTrackDbContextHelper = timeTrackDbContextHelper;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ParsingStatusEntity>(b =>
            {
                b.HasKey(x => new {x.FileId, x.Source});

                b.Property(x => x.Md5).IsRequired();
            });
            
            modelBuilder.Entity<SourceActualizingStateEntity>(b =>
            {
                b.HasKey(x => x.Source);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _timeTrackDbContextHelper.OnBeforeSaveChanges(ChangeTracker);

            return base.SaveChangesAsync(cancellationToken);
        }

        public async Task<IDisposable> CreateTransaction(IsolationLevel isolationLevel) 
            => await Database.BeginTransactionAsync(isolationLevel);

        public void CommitTransaction()
        {
            var currentTransaction = Database.CurrentTransaction;

            if (currentTransaction == null)
                throw new Exception("Can't commit empty transaction.");

            currentTransaction.Commit();
        }
    }
}
