using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Lilin.MessageContracts;

namespace ImoutoRebirth.Arachne.Service.Extensions;

internal static class SearchEngineTypeExtensions
{
    public static MetadataSource GetMetadataSource(this SearchEngineType searchEngineType)
    {
        switch (searchEngineType)
        {
            case SearchEngineType.Yandere:
                return MetadataSource.Yandere;
            case SearchEngineType.Danbooru:
                return MetadataSource.Danbooru;
            case SearchEngineType.Sankaku:
                return MetadataSource.Sankaku;
            case SearchEngineType.Gelbooru:
                return MetadataSource.Gelbooru;
            case SearchEngineType.Rule34:
                return MetadataSource.Rule34;
            case SearchEngineType.ExHentai:
                return MetadataSource.ExHentai;
            case SearchEngineType.Schale:
                return MetadataSource.Schale;
            default:
                throw new ArgumentOutOfRangeException(nameof(searchEngineType), searchEngineType, null);
        }
    }

    public static SearchEngineType ToModel(this MessageContracts.SearchEngineType searchEngineType)
    {
        switch (searchEngineType)
        {
            case MessageContracts.SearchEngineType.Yandere:
                return SearchEngineType.Yandere;
            case MessageContracts.SearchEngineType.Danbooru:
                return SearchEngineType.Danbooru;
            case MessageContracts.SearchEngineType.Sankaku:
                return SearchEngineType.Sankaku;
            case MessageContracts.SearchEngineType.Gelbooru:
                return SearchEngineType.Gelbooru;
            case MessageContracts.SearchEngineType.Rule34:
                return SearchEngineType.Rule34;
            case MessageContracts.SearchEngineType.ExHentai:
                return SearchEngineType.ExHentai;
            case MessageContracts.SearchEngineType.Schale:
                return SearchEngineType.Schale;
            default:
                throw new ArgumentOutOfRangeException(nameof(searchEngineType), searchEngineType, null);
        }
    }
}
