using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Service.Consumers;
using ImoutoRebirth.Arachne.Service.SearchEngineHistory;
using ImoutoRebirth.Common.MassTransit;
using MassTransit;
using MassTransit.Transports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Arachne.Host;

internal class AvailabilityCheckerBackgroundService : BackgroundService
{
    private readonly IEnumerable<IAvailabilityProvider> _availabilityProviders;
    private readonly IBusControl _busControl;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AvailabilityCheckerBackgroundService> _logger;

    private readonly Dictionary<SearchEngineType, HostReceiveEndpointHandle> _endpointHandles = new();

    private HostReceiveEndpointHandle? _tagHistoryEndpointHandler;
    private HostReceiveEndpointHandle? _notesHistoryEndpointHandler;

    public AvailabilityCheckerBackgroundService(
        IEnumerable<IAvailabilityProvider> availabilityProviders,
        IBusControl busControl,
        ILogger<AvailabilityCheckerBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _availabilityProviders = availabilityProviders;
        _busControl = busControl;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var ct = stoppingToken;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await CheckAvailability(ct);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "An exception occured in checking availability statuses");
            }

            await Task.Delay(15 * 1000, ct);
        }
    }

    private async Task CheckAvailability(CancellationToken ct)
    {
        var enginesIsAvailable = new Dictionary<SearchEngineType, bool>();
        foreach (var availabilityProvider in _availabilityProviders)
        {
            var booruType = availabilityProvider.ForType;
            var availabilityChecker = availabilityProvider.CreateAvailabilityChecker();
            var isAvailable = await availabilityChecker.IsAvailable(ct);

            if (isAvailable)
            {
                StartEndpoint(booruType);
            }
            else
            {
                await StopEndpoint(booruType, ct);
            }
            enginesIsAvailable[booruType] = isAvailable;
        }
        
        if (enginesIsAvailable[SearchEngineType.Danbooru] && enginesIsAvailable[SearchEngineType.Yandere])
        {
            StartHistoryEndpoint();
        }
        else
        {
            await StopHistoryEndpoint(ct);
        }
    }

    private void StartEndpoint(SearchEngineType booruType)
    {
        if (_endpointHandles.TryGetValue(booruType, out var endpointHandle))
        {
            var isStarted = endpointHandle.ReceiveEndpoint is ReceiveEndpoint
            {
                CurrentState: ReceiveEndpoint.State.Ready
            };
                    
            if (!isStarted)
            {
                _logger.LogInformation(
                    "Receive endpoint for {BooruLoaderFabric} is starting",
                    booruType);
                endpointHandle.ReceiveEndpoint.Start();
            }
        }
        else
        {
            _logger.LogInformation("Receive endpoint for {BooruLoaderFabric} is creating", booruType);
            var handle = booruType switch
            {
                SearchEngineType.Yandere => _busControl.ConnectNewConsumer<YandereSearchMetadataCommandConsumer>(_serviceProvider),
                SearchEngineType.Danbooru => _busControl.ConnectNewConsumer<DanbooruSearchMetadataCommandConsumer>(_serviceProvider),
                SearchEngineType.Sankaku => _busControl.ConnectNewConsumer<SankakuSearchMetadataCommandConsumer>(_serviceProvider),
                SearchEngineType.Gelbooru => _busControl.ConnectNewConsumer<GelbooruSearchMetadataCommandConsumer>(_serviceProvider),
                SearchEngineType.Rule34 => _busControl.ConnectNewConsumer<Rule34SearchMetadataCommandConsumer>(_serviceProvider),
                SearchEngineType.ExHentai => _busControl.ConnectNewConsumer<ExHentaiSearchMetadataCommandConsumer>(_serviceProvider),
                SearchEngineType.Schale => _busControl.ConnectNewConsumer<SchaleSearchMetadataCommandConsumer>(_serviceProvider),
                _ => throw new ArgumentOutOfRangeException()
            };
            _endpointHandles[booruType] = handle;
        }
    }

    private async Task StopEndpoint(SearchEngineType booruType, CancellationToken ct)
    {
        if (!_endpointHandles.TryGetValue(booruType, out var endpointHandle))
            return;

        var isStarted = endpointHandle.ReceiveEndpoint is ReceiveEndpoint
        {
            CurrentState: ReceiveEndpoint.State.Ready
        };

        if (isStarted)
        {
            _logger.LogInformation(
                "Receive endpoint for {BooruLoaderFabric} is stopping",
                booruType);
            await endpointHandle.ReceiveEndpoint.Stop(ct);
        }
    }

    private void StartHistoryEndpoint()
    {
        if (_tagHistoryEndpointHandler is not null)
        {
            var isStarted = _tagHistoryEndpointHandler.ReceiveEndpoint is ReceiveEndpoint
            {
                CurrentState: ReceiveEndpoint.State.Ready
            };
                    
            if (!isStarted)
            {
                _logger.LogInformation("Tag history endpoint is starting");
                _tagHistoryEndpointHandler.ReceiveEndpoint.Start();
            }
        }
        else
        {
            _logger.LogInformation("Tag history endpoint is creating");
            var handle = _busControl.ConnectNewConsumer<LoadTagHistoryCommandConsumer>(_serviceProvider);
            _tagHistoryEndpointHandler = handle;
        }
        
        if (_notesHistoryEndpointHandler is not null)
        {
            var isStarted = _notesHistoryEndpointHandler.ReceiveEndpoint is ReceiveEndpoint
            {
                CurrentState: ReceiveEndpoint.State.Ready
            };
                    
            if (!isStarted)
            {
                _logger.LogInformation("Notes history endpoint is starting");
                _notesHistoryEndpointHandler.ReceiveEndpoint.Start();
            }
        }
        else
        {
            _logger.LogInformation("Notes history endpoint is creating");
            var handle = _busControl.ConnectNewConsumer<LoadNoteHistoryCommandConsumer>(_serviceProvider);
            _notesHistoryEndpointHandler = handle;
        }
    }
    
    private async Task StopHistoryEndpoint(CancellationToken ct)
    {
        if (_tagHistoryEndpointHandler is not null)
        {
            var isStarted = _tagHistoryEndpointHandler.ReceiveEndpoint is ReceiveEndpoint
            {
                CurrentState: ReceiveEndpoint.State.Ready
            };

            if (isStarted)
            {
                _logger.LogInformation("Tag history endpoint is stopping");
                await _tagHistoryEndpointHandler.ReceiveEndpoint.Stop(ct);
            }
        }
        
        if (_notesHistoryEndpointHandler is not null)
        {
            var isStarted = _notesHistoryEndpointHandler.ReceiveEndpoint is ReceiveEndpoint
            {
                CurrentState: ReceiveEndpoint.State.Ready
            };

            if (isStarted)
            {
                _logger.LogInformation("Notes history endpoint is stopping");
                await _notesHistoryEndpointHandler.ReceiveEndpoint.Stop(ct);
            }
        }
    }
}
