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
        CancellationToken cancellationToken, 
        StreamHandlerDelegate<TResponse> next)
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
        catch (Exception e)
        {
            _logger.LogError(e, "Error in handling {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
