using Flurl.Http;
using Flurl.Http.Configuration;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;

namespace ImoutoRebirth.Arachne.Infrastructure.Availability;

internal class SimpleAvailabilityChecker : IBooruAvailabilityChecker
{
    private readonly IFlurlClientFactory _clientFactory;
    private readonly Uri _domain;

    public SimpleAvailabilityChecker(IFlurlClientFactory clientFactory, Uri domain)
    {
        _clientFactory = clientFactory;
        _domain = domain;
    }

    public async Task<bool> IsAvailable(CancellationToken ct)
    {
        var client = _clientFactory.Get(_domain);

        var isHostAvailable = false;
        try
        {
            var response = await client
                .WithTimeout(TimeSpan.FromSeconds(15))
                .WithHeader("User-Agent", "Arachne/4.23.1.0")
                .Request()
                .HeadAsync(cancellationToken: ct);

            isHostAvailable = response.StatusCode / 100 == 2;
        }
        catch
        {
            // ignored
        }

        return isHostAvailable;
    }
}
