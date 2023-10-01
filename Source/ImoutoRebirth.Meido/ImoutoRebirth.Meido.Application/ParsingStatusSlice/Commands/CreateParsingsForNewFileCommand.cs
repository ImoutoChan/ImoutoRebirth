using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain;
using ImoutoRebirth.Meido.Domain.ParsingStatusAggregate;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace ImoutoRebirth.Meido.Application.ParsingStatusSlice.Commands;

public record CreateParsingsForNewFileCommand(Guid FileId, string Md5) : ICommand;

internal class CreateParsingsForNewFileCommandHandler : ICommandHandler<CreateParsingsForNewFileCommand>
{
    private readonly IParsingStatusRepository _parsingStatusRepository;
    private readonly IEventStorage _eventStorage;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public CreateParsingsForNewFileCommandHandler(
        IParsingStatusRepository parsingStatusRepository,
        IEventStorage eventStorage,
        IClock clock,
        ILogger<CreateParsingsForNewFileCommandHandler> logger)
    {
        _parsingStatusRepository = parsingStatusRepository;
        _eventStorage = eventStorage;
        _clock = clock;
        _logger = logger;
    }

    public async Task Handle(CreateParsingsForNewFileCommand request, CancellationToken ct)
    {
        var (fileId, md5) = request;
        var now = _clock.GetCurrentInstant();
        
        // we're ignoring gelbooru since it's unnecessary when danbooru entry is present
        var allMetadataSources = new[] { MetadataSource.Danbooru, MetadataSource.Yandere, MetadataSource.Sankaku };

        foreach (var metadataSource in allMetadataSources)
        {
            var check = await _parsingStatusRepository.Get(fileId, metadataSource);
            if (check != null)
            {
                _logger.LogWarning(
                    "Can't create a parsing status with duplicate key {FileId}, {Source}",
                    fileId,
                    metadataSource);
                continue;
            }

            var parsingStatus = ParsingStatus.Create(fileId, md5, metadataSource, now);
                
            await _parsingStatusRepository.Add(parsingStatus.Result);
            _eventStorage.AddRange(parsingStatus.EventsCollection);
        }
    }
}
