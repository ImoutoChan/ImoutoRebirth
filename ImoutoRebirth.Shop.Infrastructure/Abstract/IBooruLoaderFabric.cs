using Imouto.BooruParser.Loaders;
using ImoutoRebirth.Shop.Core.Models;

namespace ImoutoRebirth.Shop.Infrastructure.Abstract
{
    internal interface IBooruLoaderFabric
    {
        SearchEngineType ForType { get; }

        IBooruAsyncLoader Create();
    }
}