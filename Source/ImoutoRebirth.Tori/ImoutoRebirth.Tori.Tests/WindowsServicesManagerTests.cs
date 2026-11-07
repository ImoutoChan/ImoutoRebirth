using System.Runtime.Versioning;
using ImoutoRebirth.Tori.Services;
using Microsoft.Win32;

namespace ImoutoRebirth.Tori.Tests;

[SupportedOSPlatform("windows")]
public sealed class WindowsServicesManagerTests : IDisposable
{
    private readonly string _keyPath = $@"Software\ImoutoRebirth.Tests\ServiceStartupTimeout\{Guid.NewGuid():N}";
    private readonly RegistryKey _controlKey;

    public WindowsServicesManagerTests()
        => _controlKey = Registry.CurrentUser.CreateSubKey(_keyPath);

    [Fact]
    public void EnsureMinimumStartupTimeout_WhenMissing_CreatesDwordWithTwoMinuteTimeout()
    {
        var timeout = WindowsServicesManager.EnsureMinimumStartupTimeout(_controlKey);

        Assert.Equal(120_000u, timeout);
        Assert.Equal(120_000, _controlKey.GetValue("ServicesPipeTimeout"));
        Assert.Equal(RegistryValueKind.DWord, _controlKey.GetValueKind("ServicesPipeTimeout"));
    }

    [Theory]
    [InlineData(30_000)]
    [InlineData(60_000)]
    public void EnsureMinimumStartupTimeout_WhenTooShort_IncreasesExistingTimeout(int previousTimeout)
    {
        _controlKey.SetValue("ServicesPipeTimeout", previousTimeout, RegistryValueKind.DWord);

        var timeout = WindowsServicesManager.EnsureMinimumStartupTimeout(_controlKey);

        Assert.Equal(120_000u, timeout);
        Assert.Equal(120_000, _controlKey.GetValue("ServicesPipeTimeout"));
    }

    [Theory]
    [InlineData(120_000)]
    [InlineData(300_000)]
    [InlineData(-1)]
    public void EnsureMinimumStartupTimeout_WhenSufficient_DoesNotReduceExistingTimeout(int previousTimeout)
    {
        _controlKey.SetValue("ServicesPipeTimeout", previousTimeout, RegistryValueKind.DWord);

        var timeout = WindowsServicesManager.EnsureMinimumStartupTimeout(_controlKey);

        Assert.Equal(unchecked((uint)previousTimeout), timeout);
        Assert.Equal(previousTimeout, _controlKey.GetValue("ServicesPipeTimeout"));
    }

    [Fact]
    public void EnsureMinimumStartupTimeout_WhenStoredAsString_ReplacesItWithDword()
    {
        _controlKey.SetValue("ServicesPipeTimeout", "300000", RegistryValueKind.String);

        var timeout = WindowsServicesManager.EnsureMinimumStartupTimeout(_controlKey);

        Assert.Equal(120_000u, timeout);
        Assert.Equal(RegistryValueKind.DWord, _controlKey.GetValueKind("ServicesPipeTimeout"));
    }

    public void Dispose()
    {
        _controlKey.Dispose();
        Registry.CurrentUser.DeleteSubKey(_keyPath);
    }
}
