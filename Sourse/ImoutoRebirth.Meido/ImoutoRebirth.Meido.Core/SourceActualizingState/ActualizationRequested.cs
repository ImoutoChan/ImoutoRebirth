using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Meido.Core.SourceActualizingState;

public class ActualizationRequested : IDomainEvent
{
    public ActualizationRequested(SourceActualizingState state)
    {
        State = state;
    }

    public SourceActualizingState State { get; }
}