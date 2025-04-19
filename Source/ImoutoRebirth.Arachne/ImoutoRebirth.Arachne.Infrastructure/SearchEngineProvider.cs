using System.Runtime.CompilerServices;
using ImoutoRebirth.Arachne.Core.InfrastructureContracts;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.ExHentai;
using ImoutoRebirth.Arachne.Infrastructure.Schale;
using Mackiovello.Maybe;

namespace ImoutoRebirth.Arachne.Infrastructure;

internal class SearchEngineProvider : ISearchEngineProvider
{
    private readonly IEnumerable<IBooruLoaderFabric> _booruLoaderFabrics;
    private readonly BooruSearchEngine.IFactory _booruSearchEngineFactory;
    private readonly ExHentaiSearchEngine _exHentaiSearchEngine;
    private readonly SchaleSearchEngine _schaleSearchEngine;
    private readonly Dictionary<SearchEngineType, ISearchEngine> _searchEngineCache;

    public SearchEngineProvider(
        IEnumerable<IBooruLoaderFabric> booruLoaderFabrics, 
        BooruSearchEngine.IFactory booruSearchEngineFactory,
        ExHentaiSearchEngine exHentaiSearchEngine,
        SchaleSearchEngine schaleSearchEngine)
    {
        _booruLoaderFabrics = booruLoaderFabrics;
        _booruSearchEngineFactory = booruSearchEngineFactory;
        _exHentaiSearchEngine = exHentaiSearchEngine;
        _schaleSearchEngine = schaleSearchEngine;
        _searchEngineCache = new Dictionary<SearchEngineType, ISearchEngine>();
    }

    public ISearchEngine Get(SearchEngineType searchEngineType) 
        => GetSearchEngineFor(searchEngineType);

    public IReadOnlyCollection<ISearchEngine> GetAll() 
        => Enum
            .GetValues<SearchEngineType>()
            .Select(Get)
            .ToArray();

    [MethodImpl(MethodImplOptions.Synchronized)]
    private ISearchEngine GetSearchEngineFor(SearchEngineType type)
    {
        if (_searchEngineCache.TryGetValue(type, out var engine))
            return engine;

        engine = CreateSearchEngine(type);
        _searchEngineCache.Add(type, engine);

        return engine;
    }

    private ISearchEngine CreateSearchEngine(SearchEngineType type)
    {
        if (type == SearchEngineType.ExHentai)
            return _exHentaiSearchEngine;

        if (type == SearchEngineType.Schale)
            return _schaleSearchEngine;

        var fabric = _booruLoaderFabrics.SingleMaybe(x => x.ForType == type);
        var loaderFabric = fabric.SelectOrElse(x => x, () => throw new NotImplementedException());
        return _booruSearchEngineFactory.Create(loaderFabric.Create(), type);
    }
}
