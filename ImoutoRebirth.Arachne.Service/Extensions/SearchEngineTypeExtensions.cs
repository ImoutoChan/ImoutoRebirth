using System;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Lilin.MessageContracts;

namespace ImoutoRebirth.Arachne.Service.Extensions
{
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(searchEngineType), searchEngineType, null);
            }
        }

        public static SearchEngineType ToModel(this MessageContracts.SearchEngineType searchEngineType) 
            => (SearchEngineType) searchEngineType;
    }
}