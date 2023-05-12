using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts;
using MassTransit;

namespace ImoutoRebirth.Arachne.Service;

public interface IMeidoReporter
{
    Task ReportSearchResultsToHeadMaid(
        ConsumeContext<ISearchMetadataCommand> context,
        SearchResult searchResults);
}