using System.Threading.Tasks;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts;
using MassTransit;

namespace ImoutoRebirth.Arachne.Service
{
    public interface ISearchMetadataCommandHandler
    {
        Task Search(ConsumeContext<ISearchMetadataCommand> context, SearchEngineType where);
    }
}