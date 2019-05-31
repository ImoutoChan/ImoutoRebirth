using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Meido.Core.SourceActualizingState;
using ImoutoRebirth.Meido.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Meido.Infrastructure
{
    public class SourceActualizingStateRepository : ISourceActualizingStateRepository
    {
        private readonly MeidoDbContext _meidoDbContext;

        public SourceActualizingStateRepository(MeidoDbContext meidoDbContext)
        {
            _meidoDbContext = meidoDbContext;
        }

        public Task Add(SourceActualizingState sourceActualizingState)
            => _meidoDbContext.SourceActualizingStates.AddAsync(sourceActualizingState);

        public async Task<IReadOnlyCollection<SourceActualizingState>> GetAll()
            => await _meidoDbContext.SourceActualizingStates.ToArrayAsync();
    }
}