using ImoutoRebirth.Room.Domain;
using ImoutoRebirth.Room.Domain.CollectionAggregate;
using Xunit;

namespace ImoutoRebirth.Room.DataAccess.Tests;

public class ImportFileSystemTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory();

    [Theory]
    [InlineData("missing")]
    [InlineData("different")]
    [InlineData("same-path")]
    [InlineData("identical")]
    public void DuplicateRemovalRequiresASeparateVerifiedCopy(string scenario)
    {
        var source = _root.CreateSubdirectory("source");
        var archive = _root.CreateSubdirectory("archive");
        var incoming = Path.Combine(source.FullName, "image.jpg");
        var retained = scenario == "same-path" ? incoming : Path.Combine(archive.FullName, "image.jpg");
        File.WriteAllText(incoming, "first");
        if (scenario is "identical" or "different")
            File.WriteAllText(retained, scenario == "identical" ? "first" : "other");

        var prepared = new SystemFilePreparedToMove(SystemFile.Create(new(incoming))!, retained);
        prepared.SetProblem(MoveProblem.AlreadyContains);
        var result = Destination(archive).Move(prepared, source.FullName);

        Assert.False(result.RequireSave);
        if (scenario == "identical" && OperatingSystem.IsWindows())
        {
            Assert.False(File.Exists(incoming));
        }
        else
        {
            Assert.Equal("first", File.ReadAllText(incoming));
        }
        if (scenario == "identical")
            Assert.Equal("first", File.ReadAllText(retained));
        if (scenario == "different")
            Assert.Equal("other", File.ReadAllText(retained));
    }

    [Fact]
    public void InPlaceQuarantineIsAnchoredAtSourceAndExcludedFromLaterScans()
    {
        var nested = _root.CreateSubdirectory("nested");
        var path = Path.Combine(nested.FullName, "image.jpg");
        File.WriteAllText(path, "contents");
        var source = new SourceFolder(Guid.NewGuid(), _root.FullName, false, true, false, false, [], false, null);
        var destination = DestinationFolder.Default;
        var candidate = Assert.Single(source.GetAllFileInfo(destination));
        var prepared = source.PrepareFileToMove(SystemFile.Create(candidate)!, null, true);

        var moved = destination.Move(prepared, _root.FullName);

        Assert.False(moved.RequireSave);
        Assert.Equal("contents", File.ReadAllText(Path.Combine(_root.FullName, "!WithoutHashError", "image.jpg")));
        Assert.Empty(source.GetAllFileInfo(destination));
    }

    [Fact]
    public void UnindexedDestinationCopyIsRegistered()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var source = _root.CreateSubdirectory("source");
        var archive = _root.CreateSubdirectory("archive");
        var incoming = Path.Combine(source.FullName, "image.jpg");
        var existing = Path.Combine(archive.FullName, "image.jpg");
        File.WriteAllText(incoming, "contents");
        File.Copy(incoming, existing);

        var prepared = new SystemFilePreparedToMove(SystemFile.Create(new(incoming))!);
        var result = Destination(archive).Move(prepared, source.FullName);

        Assert.True(result.RequireSave);
        Assert.Equal(existing, result.MovedFileInfo.FullName);
        Assert.Equal("contents", File.ReadAllText(existing));
        Assert.False(File.Exists(incoming));
    }

    private static DestinationFolder Destination(DirectoryInfo archive)
        => new(Guid.NewGuid(), archive.FullName, false, false, "!format", "!hash", "!missing");

    public void Dispose() => _root.Delete(recursive: true);
}
