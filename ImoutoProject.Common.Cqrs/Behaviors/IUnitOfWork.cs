using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ImoutoProject.Common.Cqrs.Behaviors
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<IDisposable> CreateTransaction(IsolationLevel isolationLevel);

        void CommitTransaction();
    }
}