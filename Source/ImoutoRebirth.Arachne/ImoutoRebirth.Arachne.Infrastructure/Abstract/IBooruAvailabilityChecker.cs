namespace ImoutoRebirth.Arachne.Infrastructure.Abstract;

public interface IBooruAvailabilityChecker
{
    Task<bool> IsAvailable(CancellationToken ct);
}
