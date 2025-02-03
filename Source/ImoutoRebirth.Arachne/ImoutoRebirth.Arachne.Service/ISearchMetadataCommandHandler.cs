using ImoutoRebirth.Arachne.MessageContracts;
using MassTransit;
using SearchEngineType = ImoutoRebirth.Arachne.Core.Models.SearchEngineType;

namespace ImoutoRebirth.Arachne.Service;

public interface ISearchMetadataCommandHandler
{
    Task Search(ConsumeContext<SearchMetadataCommand> context, SearchEngineType where);
}
