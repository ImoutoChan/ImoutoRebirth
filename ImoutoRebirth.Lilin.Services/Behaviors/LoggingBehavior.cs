using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Lilin.Services.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            _logger.LogTrace("Handling {RequestName}", typeof(TRequest).Name);
            try
            {
                var response = await next();

                _logger.LogTrace(
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
}