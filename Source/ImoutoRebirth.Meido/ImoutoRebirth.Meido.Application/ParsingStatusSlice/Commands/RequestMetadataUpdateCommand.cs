using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain;
using NodaTime;

namespace ImoutoRebirth.Meido.Application.ParsingStatusSlice.Commands;

public record RequestMetadataUpdateCommand(IReadOnlyCollection<int> PostIds, MetadataSource Source) : ICommand;
    
internal class RequestMetadataUpdateCommandHandler : ICommandHandler<RequestMetadataUpdateCommand>
{
    private readonly IClock _clock;
    private readonly IParsingStatusRepository _parsingStatusRepository;
    private readonly IEventStorage _eventStorage;

    public RequestMetadataUpdateCommandHandler(
        IParsingStatusRepository parsingStatusRepository,
        IEventStorage eventStorage,
        IClock clock)
    {
        _parsingStatusRepository = parsingStatusRepository;
        _eventStorage = eventStorage;
        _clock = clock;
    }

    public async Task Handle(RequestMetadataUpdateCommand command, CancellationToken ct)
    {
        var (postIds, source) = command;

        var now = _clock.GetCurrentInstant();

        var posts = await _parsingStatusRepository.GetBySourcePostIds(postIds, source);

        foreach (var parsingStatus in posts)
        {
            var result = parsingStatus.RequestMetadataUpdate(now);
            _eventStorage.AddRange(result.EventsCollection);
        }
    }
}
