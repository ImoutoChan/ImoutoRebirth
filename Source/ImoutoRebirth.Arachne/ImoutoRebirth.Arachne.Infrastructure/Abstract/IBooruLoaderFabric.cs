using Imouto.BooruParser;
using ImoutoRebirth.Arachne.Core.Models;

namespace ImoutoRebirth.Arachne.Infrastructure.Abstract;

public interface IBooruLoaderFabric
{
    SearchEngineType ForType { get; }

    IBooruApiLoader Create();

    IBooruAvailabilityChecker CreateAvailabilityChecker();
}
