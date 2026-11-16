using System.Diagnostics;
using System.IO;
using AwesomeAssertions;
using ImoutoRebirth.Navigator.Services;
using Xunit;

namespace ImoutoRebirth.Navigator.Tests;

public class VlcLibraryImagesTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"NavigatorVideoTests-{Guid.NewGuid():N}");

    [Fact]
    public async Task PrepareAsync_ShouldHoldImagesWithoutLoadingLibrariesAndReleaseThemOnDispose()
    {
        var library = CopyLibrary();
        using var images = await VlcLibraryImages.PrepareAsync(_directory);

        using var process = Process.GetCurrentProcess();
        process.Modules.Cast<ProcessModule>().Should().NotContain(x => x.FileName == library);
        Action openForWriting = () =>
        {
            using var file = File.OpenHandle(library, FileMode.Open, FileAccess.Write);
        };
        openForWriting.Should().Throw<IOException>();

        images.Dispose();

        openForWriting.Should().NotThrow();
    }

    [Fact]
    public async Task PrepareAsync_ShouldContinueWithValidLibrariesWhenAnotherImageIsInvalid()
    {
        var library = CopyLibrary();
        await File.WriteAllTextAsync(Path.Combine(_directory, "invalid.dll"), "Not an executable image");

        using var images = await VlcLibraryImages.PrepareAsync(_directory);

        Action openForWriting = () =>
        {
            using var file = File.OpenHandle(library, FileMode.Open, FileAccess.Write);
        };
        openForWriting.Should().Throw<IOException>();
    }

    [Fact]
    public async Task PrepareAsync_ShouldAllowNormalStartupWhenDirectoryIsMissing()
    {
        using var images = await VlcLibraryImages.PrepareAsync(_directory);
    }

    private string CopyLibrary()
    {
        var pluginDirectory = Path.Combine(_directory, "plugins");
        Directory.CreateDirectory(pluginDirectory);
        var library = Path.Combine(pluginDirectory, $"{Guid.NewGuid():N}.dll");
        File.Copy(Path.Combine(Environment.SystemDirectory, "kernel32.dll"), library);
        return library;
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, true);
    }
}