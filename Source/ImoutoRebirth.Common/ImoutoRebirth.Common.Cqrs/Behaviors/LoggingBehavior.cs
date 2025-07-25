﻿using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Common.Cqrs.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken ct)
    {
        _logger.LogTrace("Handling {RequestName}", typeof(TRequest).Name);
        try
        {
            var sw = new Stopwatch();
            sw.Start();
            var response = await next(ct);
            sw.Stop();

            _logger.LogDebug(
                "Handled {RequestName} with response {ResponseName} by {HandleTime}",
                typeof(TRequest).Name,
                typeof(TResponse).Name,
                sw.ElapsedMilliseconds);
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
