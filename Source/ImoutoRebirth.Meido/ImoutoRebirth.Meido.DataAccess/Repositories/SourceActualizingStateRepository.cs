using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain;
using ImoutoRebirth.Meido.Domain.SourceActualizingStateAggregate;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Meido.DataAccess.Repositories;

internal class SourceActualizingStateRepository : ISourceActualizingStateRepository
{
    private readonly MeidoDbContext _meidoDbContext;

    public SourceActualizingStateRepository(MeidoDbContext meidoDbContext) => _meidoDbContext = meidoDbContext;

    public async Task<IReadOnlyCollection<SourceActualizingState>> GetAll()
        => await _meidoDbContext.SourceActualizingStates.ToListAsync();

    public async Task<SourceActualizingState> Get(MetadataSource forSource)
        => await _meidoDbContext.SourceActualizingStates.Where(x => x.Source == forSource).FirstAsync();
}
