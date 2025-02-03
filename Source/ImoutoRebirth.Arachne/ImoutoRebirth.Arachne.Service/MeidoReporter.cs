using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Meido.MessageContracts;
using MassTransit;

namespace ImoutoRebirth.Arachne.Service;

public class MeidoReporter : IMeidoReporter
{
    public Task ReportSearchResultsToHeadMaid(
        ConsumeContext<SearchMetadataCommand> context,
        SearchResult searchResults)
    {
        var command = searchResults switch
        {
            Metadata metadata => new SearchCompleteCommand(
                context.Message.FileId,
                (int)metadata.Source,
                metadata.IsFound ? SearchStatus.Success : SearchStatus.NotFound,
                null,
                metadata.FileIdFromSource),
            SearchError error => new SearchCompleteCommand(
                context.Message.FileId,
                (int)error.Source,
                SearchStatus.Error,
                error.Error),
            _ => throw new ArgumentOutOfRangeException(nameof(searchResults))
        };

        return context.Send(command);
    }
}
