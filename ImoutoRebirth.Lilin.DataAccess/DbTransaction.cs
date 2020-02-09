using System.Threading.Tasks;
using ImoutoRebirth.Common.Domain;
using Microsoft.EntityFrameworkCore.Storage;

namespace ImoutoRebirth.Lilin.DataAccess
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