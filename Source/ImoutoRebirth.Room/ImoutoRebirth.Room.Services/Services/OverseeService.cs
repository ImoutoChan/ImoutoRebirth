using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Core.Services;

public class OverseeService : IOverseeService
{
    private readonly IFileSystemActualizationService _fileSystemActualizationService;
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILogger _logger;
    private static readonly SemaphoreSlim SemaphoreSlim = new(1);

    public OverseeService(
        IFileSystemActualizationService fileSystemActualizationService,
        ICollectionRepository collectionRepository,
        ILogger<OverseeService> logger)
    {
        _fileSystemActualizationService = fileSystemActualizationService;
        _collectionRepository = collectionRepository;
        _logger = logger;
    }

    public async Task Oversee()
    {
        if (!await SemaphoreSlim.WaitAsync(0))
        {
            _logger.LogTrace("Oversee process have not finished yet");
            return;
        }

        var runMoreTimes = 0;
        do
        {
            var anyFileMoved = false;
            try
            {
                var collections = await LoadCollections();

                foreach (var oversawCollection in collections)
                    anyFileMoved |= await _fileSystemActualizationService.PryCollection(oversawCollection);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Oversee process error");
                break;
            }

            if (anyFileMoved)
                runMoreTimes = 10;

            runMoreTimes--;

            if (runMoreTimes > 0)
                await Task.Delay(500);

        } while (runMoreTimes > 0);
            
            
        SemaphoreSlim.Release();
    }

    private async Task<IReadOnlyCollection<OversawCollection>> LoadCollections()
    {
        return await _collectionRepository.GetAllOversaw();
    }
}
