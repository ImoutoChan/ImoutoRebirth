using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain;
using NodaTime;

namespace ImoutoRebirth.Meido.Application.ParsingStatusSlice.Commands;

public record MarkMetadataSavedCommand(Guid FileId, MetadataSource Source) : ICommand;

internal class MarkMetadataSavedCommandHandler : ICommandHandler<MarkMetadataSavedCommand>
{
    private readonly IClock _clock;
    private readonly IEventStorage _eventStorage;
    private readonly IParsingStatusRepository _parsingStatusRepository;

    public MarkMetadataSavedCommandHandler(
        IParsingStatusRepository parsingStatusRepository,
        IEventStorage eventStorage,
        IClock clock)
    {
        _parsingStatusRepository = parsingStatusRepository;
        _eventStorage = eventStorage;
        _clock = clock;
    }

    public async Task Handle(MarkMetadataSavedCommand request, CancellationToken ct)
    {
        var (fileId, source) = request;

        ArgumentValidator.Requires(() => fileId != default, nameof(fileId));
        ArgumentValidator.IsEnumDefined(() => source);

        var now = _clock.GetCurrentInstant();
        var parsingStatus = await _parsingStatusRepository.Get(fileId, source)
            .GetAggregateOrThrow($"{fileId}, {source}");

        var result = parsingStatus.SetSearchSaved(now);
        _eventStorage.AddRange(result.EventsCollection);
    }
}
