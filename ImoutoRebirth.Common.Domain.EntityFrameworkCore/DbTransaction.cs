using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace ImoutoRebirth.Common.Domain.EntityFrameworkCore
{
    public class DbTransaction : ITransaction
    {
        private readonly IDbContextTransaction _dbContextTransaction;

        public DbTransaction(IDbContextTransaction dbContextTransaction)
        {
            _dbContextTransaction = dbContextTransaction;
        }

        public void Dispose()
        {
            _dbContextTransaction.Dispose();
        }

        public Task CommitAsync() => _dbContextTransaction.CommitAsync();
    }
}
