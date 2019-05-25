using System.Threading.Tasks;
using ImoutoRebirth.Meido.Core;
using ImoutoRebirth.Meido.Core.SourceActualizingState;
using ImoutoRebirth.Meido.DataAccess;

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
    }
}