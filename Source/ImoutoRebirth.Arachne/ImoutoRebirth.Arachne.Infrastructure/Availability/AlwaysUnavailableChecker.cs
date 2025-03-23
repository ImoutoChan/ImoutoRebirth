using ImoutoRebirth.Arachne.Infrastructure.Abstract;

namespace ImoutoRebirth.Arachne.Infrastructure.Availability;

internal class AlwaysUnavailableChecker : IAvailabilityChecker
{
    public Task<bool> IsAvailable(CancellationToken ct) => Task.FromResult(false);
}
