using System;
using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.DataAccess;

namespace ImoutoRebirth.Lilin.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LilinDbContext _context;

        public UnitOfWork(LilinDbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
            => _context.SaveChangesAsync(cancellationToken);


        public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _context
        }

        public void Dispose() => _context.Dispose();
    }
}
