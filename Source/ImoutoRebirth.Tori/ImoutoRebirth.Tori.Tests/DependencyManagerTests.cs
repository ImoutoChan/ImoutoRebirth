using AwesomeAssertions;
using ImoutoRebirth.Common.Tests;
using ImoutoRebirth.Tori.Services;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace ImoutoRebirth.Tori.Tests;

public class DependencyManagerTests
{
    private readonly ITestOutputHelper _output;

    public DependencyManagerTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void DependencyManager_WhenCalledOnLocal_ShouldReturnVersions()
    {
        var manager = new DependencyManager(
            TestOutputLogger.GetLogger<DependencyManager>(_output),
            Options.Create(
                new DependencyManagerOptions
                {
                    ProcessConsoleOutput = s => _output.WriteLine(s)
                }));

        var postgresServices = manager.GetPostgresWindowsServices();
        var isPostgresPortInUse = manager.IsPostgresPortInUse();
        var isPostgresInstalled = manager.IsPostgresInstalled();

        var isDotnetAspNetRuntimeInstalled = manager.IsDotnetAspNetRuntimeInstalled("9.0.6");
        var isDotnetDesktopRuntimeInstalled = manager.IsDotnetDesktopRuntimeInstalled("9.0.6");
        var dotnetRuntimes = manager.GetDotnetRuntimes();

        postgresServices.Should().NotBeEmpty();
        isPostgresPortInUse.Should().BeTrue();
        isPostgresInstalled.Should().BeTrue();
        isDotnetAspNetRuntimeInstalled.Should().BeTrue();
        isDotnetDesktopRuntimeInstalled.Should().BeTrue();
        dotnetRuntimes.Should().NotBeEmpty();
    }

    [Fact(Skip = "Requires admin privileges and is long running")]
    public void DependencyManager_WhenCalledOnLocal_ShouldInstallPostgres()
    {
        var manager = new DependencyManager(
            TestOutputLogger.GetLogger<DependencyManager>(_output),
            Options.Create(
                new DependencyManagerOptions
                {
                    ProcessConsoleOutput = s => _output.WriteLine(s)
                }))
        {
            IsDryRun = true
        };

        manager.InstallPostgres();
        manager.InstallDotnetAspNetRuntime("9.0.6");
        manager.InstallDotnetDesktopRuntime("9.0.6");
    }
}
