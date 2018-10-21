using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ImoutoRebirth.Arachne.Core.InfrastructureContracts;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using Mackiovello.Maybe;

namespace ImoutoRebirth.Arachne.Infrastructure
{
    internal class SearchEngineProvider : ISearchEngineProvider
    {
        private readonly IEnumerable<IBooruLoaderFabric> _booruLoaderFabrics;
        private readonly BooruSearchEngine.IFactory _booruSearchEngineFactory;
        private readonly Dictionary<SearchEngineType, ISearchEngine> _searchEngineCache;

        public SearchEngineProvider(
            IEnumerable<IBooruLoaderFabric> booruLoaderFabrics, 
            BooruSearchEngine.IFactory booruSearchEngineFactory)
        {
            _booruLoaderFabrics = booruLoaderFabrics;
            _booruSearchEngineFactory = booruSearchEngineFactory;
            _searchEngineCache = new Dictionary<SearchEngineType, ISearchEngine>();
        }

        public ISearchEngine Get(SearchEngineType searchEngineType) 
            => GetSearchEngineFor(searchEngineType);

        public IReadOnlyCollection<ISearchEngine> GetAll() 
            => Enum
              .GetValues(typeof(SearchEngineType))
              .Cast<SearchEngineType>()
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
            var fabric = _booruLoaderFabrics.SingleMaybe(x => x.ForType == type);
            var loaderFabric = fabric.SelectOrElse(x => x, () => throw new NotImplementedException());
            return _booruSearchEngineFactory.Create(loaderFabric.Create(), type);
        }
    }
}