using Imouto.BooruParser;
using ImoutoRebirth.Arachne.Core.Models;

namespace ImoutoRebirth.Arachne.Infrastructure.Abstract;

internal interface IBooruLoaderFabric
{
    SearchEngineType ForType { get; }

    IBooruApiLoader Create();
}
