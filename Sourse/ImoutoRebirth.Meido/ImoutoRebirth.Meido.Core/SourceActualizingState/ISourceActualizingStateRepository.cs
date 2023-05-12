namespace ImoutoRebirth.Meido.Core.SourceActualizingState;

public interface ISourceActualizingStateRepository
{
    Task Add(SourceActualizingState parsingStatus);

    Task<IReadOnlyCollection<SourceActualizingState>> GetAll();
}