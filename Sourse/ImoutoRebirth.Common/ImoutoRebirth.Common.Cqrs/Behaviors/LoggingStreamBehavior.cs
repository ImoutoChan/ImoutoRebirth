using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Common.Cqrs.Behaviors;

public class LoggingStreamBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    private readonly ILogger<LoggingStreamBehavior<TRequest, TResponse>> _logger;

    public LoggingStreamBehavior(ILogger<LoggingStreamBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public IAsyncEnumerable<TResponse> Handle(
        TRequest request, 
        StreamHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        _logger.LogTrace("Handling {RequestName}", typeof(TRequest).Name);
        try
        {
            var response = next();

            _logger.LogInformation(
                "Handled {RequestName} with response {ResponseName}",
                typeof(TRequest).Name,
                typeof(TResponse).Name);
            
            return response;
        }
        catch (OperationCanceledException e)
        {
            _logger.LogWarning(e, "Handling cancelled for {RequestName}", typeof(TRequest).Name);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in handling {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
