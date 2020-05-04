using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Common.Domain.EntityFrameworkCore;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using ImoutoRebirth.Meido.Core.SourceActualizingState;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Meido.DataAccess
{
    public class MeidoDbContext : DbContext, IUnitOfWork
    {
        private readonly IEventStorage _eventStorage;

        public DbSet<ParsingStatus> ParsingStatuses { get; set; } = default!;

        public DbSet<SourceActualizingState> SourceActualizingStates { get; set; } = default!;

        public MeidoDbContext(
            DbContextOptions<MeidoDbContext> options,
            IEventStorage eventStorage)
            : base(options)
        {
            _eventStorage = eventStorage;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeidoDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ChangeTracker.TrackShadowedTimeBeforeSaveChanges();
            return base.SaveChangesAsync(cancellationToken);
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

        public async Task<ITransaction> CreateTransactionAsync(IsolationLevel isolationLevel)
        {
            if (Database.CurrentTransaction != null)
                return new EmptyTransaction();

            return new DbTransaction(await Database.BeginTransactionAsync(isolationLevel));
        }
    }
}
