using System.Text.Json;
using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Application;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lamia.Application.Services;
using ImoutoRebirth.Lilin.MessageContracts;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Lamia.Application.Cqrs;

public record ExtractMetadataCommand(Guid FileId, string FileFullName) : ICommand;

internal class CreateParsingsForNewFileCommandHandler : ICommandHandler<ExtractMetadataCommand>
{
    private readonly IMetadataProvider _metadataProvider;
    private readonly ILogger _logger;
    private readonly IDistributedCommandBus _distributedCommandBus;

    public CreateParsingsForNewFileCommandHandler(
        IMetadataProvider metadataProvider,
        ILogger<CreateParsingsForNewFileCommandHandler> logger,
        IDistributedCommandBus distributedCommandBus)
    {
        _metadataProvider = metadataProvider;
        _logger = logger;
        _distributedCommandBus = distributedCommandBus;
    }

    public async Task Handle(ExtractMetadataCommand command, CancellationToken ct)
    {
        var (fileId, fileFullName) = command;

        try
        {
            _logger.LogInformation("Extracting metadata for {FileFullName}", fileFullName);
            var metadata = await _metadataProvider.GetMetadata(fileFullName);

            var extractedTags = metadata.ExtractTags().Distinct().ToList();

            var tags = extractedTags
                .Select(x => new FileTag("Meta", x.tagName, x.tagValue, null))
                .ToArray();

            _logger.LogInformation(
                "Metadata extracted for {FileFullName} with tags:\n{Tags}",
                fileFullName,
                DefaultJsonSerializer.SerializeIndented(extractedTags));

            await _distributedCommandBus.SendAsync(
                new UpdateMetadataCommand(fileId, MetadataSource.Lamia, [], tags),
                ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception occured while extracting metadata");
        }
    }
}
