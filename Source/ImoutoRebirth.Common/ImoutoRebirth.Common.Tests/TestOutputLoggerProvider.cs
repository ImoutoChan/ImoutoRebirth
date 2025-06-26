using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace ImoutoRebirth.Common.Tests;

public class TestOutputLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;

    public TestOutputLoggerProvider(ITestOutputHelper output) => _output = output;

    public ILogger CreateLogger(string categoryName) => new TestOutputLogger(_output, categoryName);

    public void Dispose()
    {
    }
}

public class TestOutputLogger : ILogger
{
    private readonly string _categoryName;
    private readonly ITestOutputHelper _output;

    public TestOutputLogger(ITestOutputHelper output, string categoryName)
    {
        _output = output;
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel, EventId eventId,
        TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        _output.WriteLine($"{formatter(state, exception)}");

        if (exception != null)
            _output.WriteLine(exception.ToString());
    }

    public static ILogger<T> GetLogger<T>(ITestOutputHelper output)
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(new TestOutputLoggerProvider(output));
        });

        return loggerFactory.CreateLogger<T>();
    }
}
