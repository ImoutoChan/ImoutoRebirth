using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Meido.Core.ParsingStatus.Events;

public class DisallowedStatusTransfer : IDomainEvent
{
    public ParsingStatus Model { get; }

    public Status NewStatus { get; }

    public DisallowedStatusTransfer(ParsingStatus model, Status newStatus)
    {
        Model = model;
        NewStatus = newStatus;
    }
}