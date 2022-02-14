using Imouto.BooruParser.Loaders;
using ImoutoRebirth.Arachne.Core.Models;

namespace ImoutoRebirth.Arachne.Infrastructure.Abstract;

internal interface IBooruLoaderFabric
{
    SearchEngineType ForType { get; }

    IBooruAsyncLoader Create();
}