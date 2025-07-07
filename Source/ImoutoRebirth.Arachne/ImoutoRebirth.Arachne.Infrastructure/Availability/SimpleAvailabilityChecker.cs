using Flurl.Http;
using Flurl.Http.Configuration;
using Imouto.BooruParser.Extensions;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;

namespace ImoutoRebirth.Arachne.Infrastructure.Availability;

internal class SimpleAvailabilityChecker : IAvailabilityChecker
{
    private readonly IFlurlClientCache _flurlClientCache;
    private readonly Uri _domain;

    public SimpleAvailabilityChecker(IFlurlClientCache flurlClientCache, Uri domain)
    {
        _flurlClientCache = flurlClientCache;
        _domain = domain;
    }

    public async Task<bool> IsAvailable(CancellationToken ct)
    {
        var client = _flurlClientCache.GetForDomain(_domain);

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
