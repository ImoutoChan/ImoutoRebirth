using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Tori;

public class CustomConsoleLogger : ILogger
{
    public CustomConsoleLogger(string _) { }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        Console.WriteLine(formatter(state, exception));
    }
}

public class CustomConsoleLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new CustomConsoleLogger(categoryName);

    public void Dispose() { }
}
