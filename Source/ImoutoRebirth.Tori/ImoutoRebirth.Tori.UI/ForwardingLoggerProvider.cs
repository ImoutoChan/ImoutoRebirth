using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Tori.UI;

public class ForwardingLoggerProvider : ILoggerProvider
{
    private readonly ForwardingLoggerInstance _forwardingLogger = new();

    public ForwardingLoggerProvider()
        => _forwardingLogger.Logged += (_, message) => Logged?.Invoke(this, message);

    public ILogger CreateLogger(string categoryName) => _forwardingLogger;

    public void Dispose()
    {
    }

    public event EventHandler<string>? Logged;

    public class ForwardingLoggerInstance : ILogger
    {
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Logged?.Invoke(this, formatter(state, exception));

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public event EventHandler<string>? Logged;
    }
}
