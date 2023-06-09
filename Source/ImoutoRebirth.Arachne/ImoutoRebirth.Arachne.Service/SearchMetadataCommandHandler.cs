﻿using System.Diagnostics;
using ImoutoRebirth.Arachne.Core;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Arachne.Service.Commands;
using ImoutoRebirth.Arachne.Service.Extensions;
using ImoutoRebirth.Lilin.MessageContracts;
using Mackiovello.Maybe;
using MassTransit;
using Microsoft.Extensions.Logging;
using SearchEngineType = ImoutoRebirth.Arachne.Core.Models.SearchEngineType;

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

    public async Task Search(ConsumeContext<ISearchMetadataCommand> context, SearchEngineType where)
    {
        var md5 = context.Message.Md5;

        _logger.LogTrace("Searching for {Md5} in {SearchEngine}", md5, where);

        var sw = new Stopwatch();
        sw.Start();
        var searchResults = await _arachneSearchService.Search(new Image(md5), where);
        sw.Stop();
            
        LogSearchResults(@where, searchResults, md5, sw);

        await SendUpdateMetadataCommand(context, searchResults);

        await _meidoReporter.ReportSearchResultsToHeadMaid(context, searchResults);
    }

    private async Task SendUpdateMetadataCommand(
        ConsumeContext<ISearchMetadataCommand> context, 
        SearchResult searchResults)
    {
        var sendCommand = ConvertToCommand(searchResults, context.Message.FileId)
            .Select(command => context.Send<IUpdateMetadataCommand>(command));

        if (sendCommand.HasValue)
            await sendCommand.Value;
    }

    private void LogSearchResults(SearchEngineType @where, SearchResult searchResults, string md5, Stopwatch sw)
    {
        switch (searchResults)
        {
            case Metadata metadata:
                _logger.LogInformation(
                    "Search result {Md5} in {SearchEngine}: {IsFound} ({Ms} ms)",
                    md5,
                    @where,
                    metadata.IsFound.ToString(),
                    sw.ElapsedMilliseconds);
                break;
            case SearchError error:
                _logger.LogWarning(
                    "Search result {Md5} in {SearchEngine}: {IsFound} {SearchErrorMessage} ({Ms} ms)",
                    md5,
                    @where,
                    "SearchError",
                    error.Error,
                    sw.ElapsedMilliseconds);
                break;
        }
    }

    private Maybe<UpdateMetadataCommand> ConvertToCommand(SearchResult searchResults, Guid fileId)
    {
        if (!(searchResults is Metadata searchResult) || !searchResult.IsFound)
            return Maybe<UpdateMetadataCommand>.Nothing;


        var tags = searchResult.Tags.Select(CreateFileTag).ToArray();
        var notes = searchResult.Notes.Select(CreateFileNote).ToArray();

        return new UpdateMetadataCommand
        {
            FileId = fileId,
            FileTags = tags,
            FileNotes = notes,
            MetadataSource = searchResult.Source.GetMetadataSource()
        }.ToMaybe();

    }

    private IFileTag CreateFileTag(Tag tag) 
        => new FileTag
        {
            Name = tag.Name,
            Type = tag.Type,
            Synonyms = tag.Synonyms.ToArray(),
            Value = tag.Value
        };

    private IFileNote CreateFileNote(Note note)
        => new FileNote
        {
            SourceId = note.SourceId,
            Label = note.Label,
            PositionFromLeft = note.Position.PointLeft,
            PositionFromTop = note.Position.PointTop,
            Width = note.Position.SizeWidth,
            Height = note.Position.SizeHeight
        };
}