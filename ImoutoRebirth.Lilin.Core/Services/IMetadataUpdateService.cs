using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Services
{
    public interface IMetadataUpdateService
    {
        Task ApplyMetadata(MetadataUpdate update);
    }
}