using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain;
using ImoutoRebirth.Meido.Domain.ParsingStatusAggregate;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace ImoutoRebirth.Meido.Application.ParsingStatusSlice.Commands;

public record RecreateParsingsForFileWithNewFileNameCommand(Guid FileId, string Md5, string NewFileName) : ICommand;

internal class RecreateParsingsForFileWithNewFileNameCommandHandler : ICommandHandler<RecreateParsingsForFileWithNewFileNameCommand>
{
    private readonly string[] _archiveExtensions =
        [".zip", ".rar", ".7z", ".tar", ".ace", ".cbz", ".cbr", ".cb7", ".cbt", ".cba"];

    private readonly IParsingStatusRepository _parsingStatusRepository;
    private readonly IEventStorage _eventStorage;
    private readonly IClock _clock;

    public RecreateParsingsForFileWithNewFileNameCommandHandler(
        IParsingStatusRepository parsingStatusRepository,
        IEventStorage eventStorage,
        IClock clock)
    {
        _parsingStatusRepository = parsingStatusRepository;
        _eventStorage = eventStorage;
        _clock = clock;
    }

    public async Task Handle(RecreateParsingsForFileWithNewFileNameCommand request, CancellationToken ct)
    {
        var (fileId, md5, newFileName) = request;
        var now = _clock.GetCurrentInstant();

        // only archive metadata parsers depend on the file name
        if (!IsArchive(newFileName))
            return;

        var allMetadataSources = new[] { MetadataSource.ExHentai };

        foreach (var metadataSource in allMetadataSources)
        {
            var existing = await _parsingStatusRepository.Get(fileId, metadataSource);
            if (existing != null)
            {
                var result = existing.Update(md5, newFileName, now);
                _eventStorage.AddRange(result.EventsCollection);
            }
            else
            {
                var parsingStatus = ParsingStatus.Create(fileId, md5, newFileName, metadataSource, now);
                await _parsingStatusRepository.Add(parsingStatus.Result);
                _eventStorage.AddRange(parsingStatus.EventsCollection);
            }
        }
    }

    private bool IsArchive(string fileName)
        => _archiveExtensions.Any(x => fileName.EndsWith(x, StringComparison.OrdinalIgnoreCase));
}
