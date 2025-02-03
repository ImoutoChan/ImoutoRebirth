using System.Diagnostics;
using ImoutoRebirth.Arachne.Core;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Arachne.Service.Extensions;
using Mackiovello.Maybe;
using MassTransit;
using Microsoft.Extensions.Logging;
using FileNote = ImoutoRebirth.Lilin.MessageContracts.FileNote;
using FileTag = ImoutoRebirth.Lilin.MessageContracts.FileTag;
using SearchEngineType = ImoutoRebirth.Arachne.Core.Models.SearchEngineType;
using UpdateMetadataCommand = ImoutoRebirth.Lilin.MessageContracts.UpdateMetadataCommand;

namespace ImoutoRebirth.Arachne.Service;

public class SearchMetadataCommandHandler : ISearchMetadataCommandHandler
{
    private readonly IArachneSearchService _arachneSearchService;
    private readonly ILogger<SearchMetadataCommandHandler> _logger;
    private readonly IMeidoReporter _meidoReporter;

    public SearchMetadataCommandHandler(
        IArachneSearchService arachneSearchService,
        ILogger<SearchMetadataCommandHandler> logger,
        IMeidoReporter meidoReporter)
    {
        _arachneSearchService = arachneSearchService;
        _logger = logger;
        _meidoReporter = meidoReporter;
    }

    public async Task Search(ConsumeContext<SearchMetadataCommand> context, SearchEngineType where)
    {
        var md5 = context.Message.Md5;
        var fileName = context.Message.FileName;

        _logger.LogTrace("Searching for {Md5} in {SearchEngine}", md5, where);

        var sw = new Stopwatch();
        sw.Start();
        var searchResults = await _arachneSearchService.Search(new Image(md5, fileName), where);
        sw.Stop();
            
        LogSearchResults(where, searchResults, md5, sw);

        await SendUpdateMetadataCommand(context, searchResults);

        await _meidoReporter.ReportSearchResultsToHeadMaid(context, searchResults);
    }

    private async Task SendUpdateMetadataCommand(
        ConsumeContext<SearchMetadataCommand> context,
        SearchResult searchResults)
    {
        var sendCommand = ConvertToCommand(searchResults, context.Message.FileId)
            .Select(command => context.Send(command));

        if (sendCommand.HasValue)
            await sendCommand.Value;
    }

    private void LogSearchResults(SearchEngineType where, SearchResult searchResults, string md5, Stopwatch sw)
    {
        switch (searchResults)
        {
            case Metadata metadata:
                _logger.LogInformation(
                    "Search result {Md5} in {SearchEngine}: {IsFound} ({Ms} ms)",
                    md5,
                    where,
                    metadata.IsFound.ToString(),
                    sw.ElapsedMilliseconds);
                break;
            case SearchError error:
                _logger.LogWarning(
                    "Search result {Md5} in {SearchEngine}: {IsFound} {SearchErrorMessage} ({Ms} ms)",
                    md5,
                    where,
                    "SearchError",
                    error.Error,
                    sw.ElapsedMilliseconds);
                break;
        }
    }

    private static Maybe<UpdateMetadataCommand> ConvertToCommand(SearchResult searchResults, Guid fileId)
    {
        if (searchResults is not Metadata { IsFound: true } searchResult)
            return Maybe<UpdateMetadataCommand>.Nothing;

        var tags = searchResult.Tags.Select(CreateFileTag).ToArray();
        var notes = searchResult.Notes.Select(CreateFileNote).ToArray();

        return new UpdateMetadataCommand(fileId, searchResult.Source.GetMetadataSource(), notes, tags).ToMaybe();

    }

    private static FileTag CreateFileTag(Tag tag) => new(tag.Type, tag.Name, tag.Value, tag.Synonyms.ToArray());

    private static FileNote CreateFileNote(Note note)
        => new(
            note.SourceId,
            note.Label,
            note.Position.PointLeft,
            note.Position.PointTop,
            note.Position.SizeWidth,
            note.Position.SizeHeight);
}
