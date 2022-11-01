using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Common.Cqrs.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        _logger.LogTrace("Handling {RequestName}", typeof(TRequest).Name);
        try
        {
            var sw = new Stopwatch();
            sw.Start();
            var response = await next();
            sw.Stop();

            _logger.LogInformation(
                "Handled {RequestName} with response {ResponseName} by {HandleTime}",
                typeof(TRequest).Name,
                typeof(TResponse).Name,
                sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in handling {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
