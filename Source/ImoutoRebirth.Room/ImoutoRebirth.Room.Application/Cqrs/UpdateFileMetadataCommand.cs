using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;
using ImoutoRebirth.Room.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Application.Cqrs;

public record UpdateFileMetadataCommand : ICommand;

internal class UpdateFileMetadataCommandHandler : ICommandHandler<UpdateFileMetadataCommand>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILogger<UpdateFileMetadataCommandHandler> _logger;
    private readonly IRemoteCommandService _remoteCommandService;
    private readonly IMediator _mediator;

    public UpdateFileMetadataCommandHandler(
        ICollectionRepository collectionRepository,
        ILogger<UpdateFileMetadataCommandHandler> logger,
        IRemoteCommandService remoteCommandService,
        IMediator mediator)
    {
        _collectionRepository = collectionRepository;
        _logger = logger;
        _remoteCommandService = remoteCommandService;
        _mediator = mediator;
    }

    public async Task Handle(UpdateFileMetadataCommand request, CancellationToken ct)
    {
        try
        {
             var collections = await _collectionRepository.GetAll(ct);

            foreach (var collection in collections)
            {
                var foundFiles = await _mediator.Send(
                    new CollectionFilesModelsQuery(new(
                        CollectionId: collection.Id,
                        CollectionFileIds: [],
                        Path: null,
                        Md5: [],
                        Count: null,
                        Skip: null)),
                    ct);

                foreach (var foundFile in foundFiles)
                {
                    await _remoteCommandService.UpdateFileMetadataRequest(foundFile.Id, foundFile.Path);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while updating local tags");
        }
    }
}
