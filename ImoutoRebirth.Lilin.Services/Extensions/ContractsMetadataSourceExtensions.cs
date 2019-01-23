using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.Extensions
{
    internal static class ContractsMetadataSourceExtensions
    {
        public static MetadataSource Convert(this MessageContracts.MetadataSource metadata)
            => (MetadataSource) (int) metadata;
    }
}
