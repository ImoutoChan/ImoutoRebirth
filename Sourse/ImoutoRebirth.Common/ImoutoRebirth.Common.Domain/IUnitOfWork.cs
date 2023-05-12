using System.Data;

namespace ImoutoRebirth.Common.Domain;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task SaveEntitiesAsync(CancellationToken cancellationToken = default);

    Task<ITransaction> CreateTransactionAsync(IsolationLevel isolationLevel);
}