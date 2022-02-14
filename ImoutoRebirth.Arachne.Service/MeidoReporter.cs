using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Meido.MessageContracts;
using MassTransit;

namespace ImoutoRebirth.Arachne.Service
{
    public class MeidoReporter : IMeidoReporter
    {
        public Task ReportSearchResultsToHeadMaid(
            ConsumeContext<ISearchMetadataCommand> context,
            SearchResult searchResults)
        {
            var command = new SearchMetadataCommand(
                context.Message.FileId, 
                (int)searchResults.Source);

            switch (searchResults)
            {
                case Metadata metadata:
                    command.ResultStatus = metadata.IsFound ? SearchStatus.Success : SearchStatus.NotFound;
                    command.FileIdFromSource = metadata.FileIdFromSource;
                    break;
                case SearchError error:
                    command.ResultStatus = SearchStatus.Error;
                    command.ErrorText = error.Error;
                    break;
            }

            return context.Send<ISearchCompleteCommand>(command);
        }

        private class SearchMetadataCommand : ISearchCompleteCommand
        {
            public SearchMetadataCommand(
                Guid fileId, 
                int sourceId)
            {
                FileId = fileId;
                SourceId = sourceId;
            }

            public Guid FileId { get; }

            public int SourceId { get; }

            public SearchStatus ResultStatus { get; set; }

            public string? ErrorText { get; set; }

            public int? FileIdFromSource { get; set; }
        }
    }
}
