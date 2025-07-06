using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Lamia.IntegrationTests.Framework;

/// <summary>
///     Inspired by WebApplicationFactory but removed everything tied to WebHost or WebHostBuilder.
///     Use for applications that are created using Host.CreateApplicationBuilder
/// </summary>
/// <typeparam name="TEntryPoint">Any type from your host assembly</typeparam>
public class GenericHostApplicationFactory<TEntryPoint> : IDisposable, IAsyncDisposable
    where TEntryPoint : class
{
    private bool _disposed;
    private bool _disposedAsync;

    private IHost? _host;

    public virtual IServiceProvider Services
    {
        get
        {
            StartServer();

            return _host?.Services ?? throw new InvalidOperationException("Host not initialized");
        }
    }

    private bool ServerStarted => _host != null;

    public virtual async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_disposedAsync)
        {
            return;
        }

        if (_host != null)
        {
            await _host.StopAsync().ConfigureAwait(false);
            _host?.Dispose();
        }

        _disposedAsync = true;

        Dispose(true);

        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~GenericHostApplicationFactory()
    {
        Dispose(false);
    }

    public void StartServer()
    {
        if (ServerStarted)
        {
            return;
        }

        EnsureDepsFile();

        var deferredHostBuilder = new DeferredHostBuilder();
        deferredHostBuilder.UseEnvironment(Environments.Development);

        // There's no helper for UseApplicationName, but we need to
        // set the application name to the target entry point
        // assembly name.
        deferredHostBuilder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    { HostDefaults.ApplicationKey, typeof(TEntryPoint).Assembly.GetName().Name ?? string.Empty }
                });
        });

        // This helper call does the hard work to determine if we can fallback to diagnostic source events to get the host instance
        var factory = HostFactoryResolver.ResolveHostFactory(
            typeof(TEntryPoint).Assembly,
            stopApplication: false,
            configureHostBuilder: deferredHostBuilder.ConfigureHostBuilder,
            entrypointCompleted: deferredHostBuilder.EntryPointCompleted);

        if (factory is not null)
        {
            // If we have a valid factory it means the specified entry point's assembly can potentially resolve the IHost
            // so we set the factory on the DeferredHostBuilder so we can invoke it on the call to IHostBuilder.Build.
            deferredHostBuilder.SetHostFactory(factory);

            ConfigureHostBuilder(deferredHostBuilder);
            return;
        }

        throw new InvalidOperationException("Microsoft.AspNetCore.Hosting.Resources.FormatMissingBuilderMethod");
    }

    private void ConfigureHostBuilder(IHostBuilder hostBuilder) => _host = CreateHost(hostBuilder);

    protected virtual IEnumerable<Assembly> GetTestAssemblies()
    {
        try
        {
            // The default dependency context will be populated in .net core applications.
            var context = DependencyContext.Default;
            if (context == null || context.CompileLibraries.Count == 0)
            {
                // The app domain friendly name will be populated in full framework.
                return [Assembly.Load(AppDomain.CurrentDomain.FriendlyName)];
            }

            var runtimeProjectLibraries = context.RuntimeLibraries
                .ToDictionary(r => r.Name, r => r, StringComparer.Ordinal);

            // Find the list of projects
            var projects = context.CompileLibraries.Where(l => l.Type == "project");

            var entryPointAssemblyName = typeof(TEntryPoint).Assembly.GetName().Name;

            // Find the list of projects referencing TEntryPoint.
            var candidates = context.CompileLibraries
                .Where(library => library.Dependencies.Any(d => string.Equals(
                    d.Name,
                    entryPointAssemblyName,
                    StringComparison.Ordinal)));

            var testAssemblies = new List<Assembly>();
            foreach (var candidate in candidates)
            {
                if (runtimeProjectLibraries.TryGetValue(candidate.Name, out var runtimeLibrary))
                {
                    var runtimeAssemblies = runtimeLibrary.GetDefaultAssemblyNames(context);
                    testAssemblies.AddRange(runtimeAssemblies.Select(Assembly.Load));
                }
            }

            return testAssemblies;
        }
        catch (Exception)
        {
            // ignored
        }

        return [];
    }

    private static void EnsureDepsFile()
    {
        if (typeof(TEntryPoint).Assembly.EntryPoint == null)
        {
            throw new InvalidOperationException(
                "Microsoft.AspNetCore.Hosting.Resources.FormatInvalidAssemblyEntryPoint");
        }

        var depsFileName = $"{typeof(TEntryPoint).Assembly.GetName().Name}.deps.json";
        var depsFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, depsFileName));
        if (!depsFile.Exists)
        {
            throw new InvalidOperationException("Microsoft.AspNetCore.Hosting.Resources.FormatMissingDepsFile");
        }
    }

    protected virtual IHost CreateHost(IHostBuilder builder)
    {
        var host = builder.Build();
        host.Start();
        return host;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            if (!_disposedAsync)
            {
                DisposeAsync()
                    .AsTask()
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }
        }

        _disposed = true;
    }
}

internal sealed class DeferredHostBuilder : IHostBuilder
{
    private readonly ConfigurationManager _hostConfiguration = new();

    // This task represents a call to IHost.Start, we create it here preemptively in case the application
    // exits due to an exception or because it didn't wait for the shutdown signal
    private readonly TaskCompletionSource _hostStartTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private Action<IHostBuilder> _configure;
    private Func<string[], object>? _hostFactory;

    public DeferredHostBuilder()
    {
        _configure = b =>
        {
            // Copy the properties from this builder into the builder
            // that we're going to receive
            foreach (var pair in Properties)
            {
                b.Properties[pair.Key] = pair.Value;
            }
        };
    }

    public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

    public IHost Build()
    {
        // Hosting configuration is being provided by args so that
        // we can impact WebApplicationBuilder based applications.
        var args = new List<string>();

        // Transform the host configuration into command line arguments
        foreach (var (key, value) in _hostConfiguration.AsEnumerable())
        {
            args.Add($"--{key}={value}");
        }

        // This will never be null if the case where Build is being called
        var host = (IHost)_hostFactory!(args.ToArray());

        // We can't return the host directly since we need to defer the call to StartAsync
        return new DeferredHost(host, _hostStartTcs);
    }

    public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        _configure += b => b.ConfigureAppConfiguration(configureDelegate);
        return this;
    }

    public IHostBuilder ConfigureContainer<TContainerBuilder>(
        Action<HostBuilderContext, TContainerBuilder> configureDelegate)
    {
        _configure += b => b.ConfigureContainer(configureDelegate);
        return this;
    }

    public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
    {
        // Run this immediately so that we can capture the host configuration
        // before we pass it to the application. We can do this for app configuration
        // as well if it becomes necessary.
        configureDelegate(_hostConfiguration);
        return this;
    }

    public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        _configure += b => b.ConfigureServices(configureDelegate);
        return this;
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        where TContainerBuilder : notnull
    {
        _configure += b => b.UseServiceProviderFactory(factory);
        return this;
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
        Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) where TContainerBuilder : notnull
    {
        _configure += b => b.UseServiceProviderFactory(factory);
        return this;
    }

    public void ConfigureHostBuilder(object hostBuilder)
    {
        _configure((IHostBuilder)hostBuilder);
    }

    public void EntryPointCompleted(Exception? exception)
    {
        // If the entry point completed we'll set the tcs just in case the application doesn't call IHost.Start/StartAsync.
        if (exception is not null)
        {
            _hostStartTcs.TrySetException(exception);
        }
        else
        {
            _hostStartTcs.TrySetResult();
        }
    }

    public void SetHostFactory(Func<string[], object> hostFactory)
    {
        _hostFactory = hostFactory;
    }

    private sealed class DeferredHost : IHost, IAsyncDisposable
    {
        private readonly IHost _host;
        private readonly TaskCompletionSource _hostStartedTcs;

        public DeferredHost(IHost host, TaskCompletionSource hostStartedTcs)
        {
            _host = host;
            _hostStartedTcs = hostStartedTcs;
        }

        public async ValueTask DisposeAsync()
        {
            if (_host is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
                return;
            }

            Dispose();
        }

        public IServiceProvider Services => _host.Services;

        public void Dispose() => _host.Dispose();

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            // Wait on the existing host to start running and have this call wait on that. This avoids starting the actual host too early and
            // leaves the application in charge of calling start.

            using var reg = cancellationToken.UnsafeRegister(_ => _hostStartedTcs.TrySetCanceled(), null);

            // REVIEW: This will deadlock if the application creates the host but never calls start. This is mitigated by the cancellationToken
            // but it's rarely a valid token for Start
            using var reg2 = _host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted
                .UnsafeRegister(_ => _hostStartedTcs.TrySetResult(), null);

            await _hostStartedTcs.Task.ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken = default) => _host.StopAsync(cancellationToken);
    }
}
