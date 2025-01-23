using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain;
using ImoutoRebirth.Meido.Domain.ParsingStatusAggregate;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace ImoutoRebirth.Meido.Application.ParsingStatusSlice.Commands;

public record SaveCompletedSearchCommand(
    MetadataSource Source,
    Guid FileId,
    SearchStatus ResultStatus,
    string? ErrorText,
    string? FileIdFromSource) : ICommand;

internal class SaveCompletedSearchCommandHandler : ICommandHandler<SaveCompletedSearchCommand>
{
    private readonly IClock _clock;
    private readonly IEventStorage _eventStorage;
    private readonly ILogger _logger;
    private readonly IParsingStatusRepository _parsingStatusRepository;

    public SaveCompletedSearchCommandHandler(
        IParsingStatusRepository parsingStatusRepository,
        IEventStorage eventStorage,
        IClock clock,
        ILogger<SaveCompletedSearchCommandHandler> logger)
    {
        _parsingStatusRepository = parsingStatusRepository;
        _eventStorage = eventStorage;
        _clock = clock;
        _logger = logger;
    }

    public async Task Handle(SaveCompletedSearchCommand request, CancellationToken ct)
    {
        var (source, fileId, searchStatus, errorText, fileIdFromSource) = request;

        await SaveSearchResult(source, fileId, searchStatus, fileIdFromSource, errorText);

        if (source == MetadataSource.Danbooru && searchStatus == SearchStatus.NotFound)
            await CreateGelbooruParsingStatus(fileId);
        
        if (source == MetadataSource.Gelbooru && searchStatus == SearchStatus.NotFound)
            await CreateRule34ParsingStatus(fileId);
    }

    private async Task SaveSearchResult(
        MetadataSource source,
        Guid fileId,
        SearchStatus resultStatus,
        string? fileIdFromSource,
        string? errorText)
    {
        var now = _clock.GetCurrentInstant();
        var parsingStatus = await _parsingStatusRepository
            .Get(fileId, source)
            .GetAggregateOrThrow($"{fileId}, {source}");

        var result = parsingStatus.SetSearchResult(resultStatus, fileIdFromSource, errorText, now);
        _eventStorage.AddRange(result.EventsCollection);
    }

    private async Task CreateGelbooruParsingStatus(Guid fileId)
    {
        var now = _clock.GetCurrentInstant();

        // we always have danbooru status for this file since it's a requirement for gelbooru
        var danbooruStatus = await _parsingStatusRepository.Get(fileId, MetadataSource.Danbooru);
        var check = await _parsingStatusRepository.Get(fileId, MetadataSource.Gelbooru);
        if (check != null || danbooruStatus == null)
        {
            _logger.LogWarning(
                "Can't create a parsing status with duplicate key {FileId}, {Source}",
                fileId,
                MetadataSource.Gelbooru);
            return;
        }

        var result = ParsingStatus.Create(fileId, danbooruStatus.Md5, MetadataSource.Gelbooru, now);
        await _parsingStatusRepository.Add(result.Result);
        _eventStorage.AddRange(result.EventsCollection);
    }

    private async Task CreateRule34ParsingStatus(Guid fileId)
    {
        var now = _clock.GetCurrentInstant();

        // we always have gelbooru status for this file since it's a requirement for rule34
        var danbooruStatus = await _parsingStatusRepository.Get(fileId, MetadataSource.Gelbooru);
        var check = await _parsingStatusRepository.Get(fileId, MetadataSource.Rule34);
        if (check != null || danbooruStatus == null)
        {
            _logger.LogWarning(
                "Can't create a parsing status with duplicate key {FileId}, {Source}",
                fileId,
                MetadataSource.Rule34);
            return;
        }

        var result = ParsingStatus.Create(fileId, danbooruStatus.Md5, MetadataSource.Rule34, now);
        await _parsingStatusRepository.Add(result.Result);
        _eventStorage.AddRange(result.EventsCollection);
    }
}
