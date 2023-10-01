using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Meido.Application.Infrastructure;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace ImoutoRebirth.Meido.Application.ParsingStatusSlice.Commands;

public record RequeueFaultedParsingsCommand : ICommand;

internal class RequeueFaultedParsingsCommandHandler : ICommandHandler<RequeueFaultedParsingsCommand>
{
    private readonly IParsingStatusRepository _parsingStatusRepository;
    private readonly IEventStorage _eventStorage;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public RequeueFaultedParsingsCommandHandler(
        IParsingStatusRepository parsingStatusRepository,
        IEventStorage eventStorage,
        IClock clock,
        ILogger<RequeueFaultedParsingsCommandHandler> logger)
    {
        _parsingStatusRepository = parsingStatusRepository;
        _eventStorage = eventStorage;
        _clock = clock;
        _logger = logger;
    }

    public async Task Handle(RequeueFaultedParsingsCommand _, CancellationToken ct)
    {
        var checkDate = _clock.GetCurrentInstant().Minus(Duration.FromDays(1));
        var faultedParsingStatuses = await _parsingStatusRepository.GetFaultedParsingStatuses(checkDate);

        _logger.LogInformation("Requesting update after faulted for {Count} posts", faultedParsingStatuses.Count);

        foreach (var parsingStatus in faultedParsingStatuses)
        {
            var result = parsingStatus.RequestMetadataUpdate(_clock.GetCurrentInstant());
            _eventStorage.AddRange(result.EventsCollection);
        }
    }
}
