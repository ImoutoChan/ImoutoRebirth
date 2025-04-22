namespace ImoutoRebirth.Arachne.Infrastructure.Abstract;

public interface IAvailabilityChecker
{
    Task<bool> IsAvailable(CancellationToken ct);
}
