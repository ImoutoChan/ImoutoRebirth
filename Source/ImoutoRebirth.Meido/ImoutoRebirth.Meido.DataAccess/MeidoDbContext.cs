using System.Data;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Common.Domain.EntityFrameworkCore;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using ImoutoRebirth.Meido.Domain.ParsingStatusAggregate;
using ImoutoRebirth.Meido.Domain.SourceActualizingStateAggregate;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Meido.DataAccess;

internal class MeidoDbContext : DbContext, IUnitOfWork
{
    private readonly IEventStorage _eventStorage;

    public required DbSet<ParsingStatus> ParsingStatuses { get; set; }

    public required DbSet<SourceActualizingState> SourceActualizingStates { get; set; }

    public MeidoDbContext(DbContextOptions<MeidoDbContext> options, IEventStorage eventStorage) : base(options)
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
