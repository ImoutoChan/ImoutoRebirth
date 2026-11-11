using Flurl.Http;
using Flurl.Http.Configuration;
using Imouto.BooruParser.Extensions;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;

namespace ImoutoRebirth.Arachne.Infrastructure.Availability;

internal class SimpleAvailabilityChecker : IAvailabilityChecker
{
    private readonly IFlurlClientCache _flurlClientCache;
    private readonly Uri _domain;
    private readonly string _userAgent;

    public SimpleAvailabilityChecker(IFlurlClientCache flurlClientCache,
        Uri domain,
        string userAgent = "Arachne/4.23.1.0")
    {
        _flurlClientCache = flurlClientCache;
        _domain = domain;
        _userAgent = userAgent;
    }

    public async Task<bool> IsAvailable(CancellationToken ct)
    {
        var client = _flurlClientCache.GetForDomain(_domain);

        var isHostAvailable = false;
        try
        {
            var response = await client
                .WithTimeout(TimeSpan.FromSeconds(15))
                .WithHeader("User-Agent", _userAgent)
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
