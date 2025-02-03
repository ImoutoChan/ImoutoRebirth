using Imouto.BooruParser;
using ImoutoRebirth.Arachne.Core.Models;

namespace ImoutoRebirth.Arachne.Infrastructure.Abstract;

public interface IAvailabilityProvider
{
    SearchEngineType ForType { get; }

    IAvailabilityChecker CreateAvailabilityChecker();
}

public interface IBooruLoaderFabric
{
    SearchEngineType ForType { get; }

    IBooruApiLoader Create();
}
