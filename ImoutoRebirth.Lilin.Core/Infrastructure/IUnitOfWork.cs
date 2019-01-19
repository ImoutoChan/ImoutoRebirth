using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ImoutoRebirth.Lilin.Core.Infrastructure
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken));
        
        Task<IDisposable> CreateTransaction(IsolationLevel isolationLevel);

        void CommitTransaction();
    }
}