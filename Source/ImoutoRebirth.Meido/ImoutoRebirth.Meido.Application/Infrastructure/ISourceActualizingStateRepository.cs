using ImoutoRebirth.Meido.Domain;
using ImoutoRebirth.Meido.Domain.SourceActualizingStateAggregate;

namespace ImoutoRebirth.Meido.Application.Infrastructure;

public interface ISourceActualizingStateRepository
{
    Task<IReadOnlyCollection<SourceActualizingState>> GetAll();

    Task<SourceActualizingState> Get(MetadataSource forSource);
}
