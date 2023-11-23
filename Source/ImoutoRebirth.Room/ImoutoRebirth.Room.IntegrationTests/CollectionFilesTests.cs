using System.Net.Http.Json;
using FluentAssertions;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Room.Application.Cqrs;
using ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ImoutoRebirth.Room.IntegrationTests;

/*
 * # What I want to test
 * + Duplicate images are ignored
 * + Images with the same md5 but different name are ignored
 * + Images with the same name, but with different md5 are processed correctly
 * 
 * # Source folder options
 * + Check flags: bad images are moved to the bad format folder
 * Check flags: images with wrong hash in the name are moved to the bad hash folder
 * Check flags: images without hash moved to the no hash folder
 * Check flags: images push its name to the tags
 * Check flags: images push its folder name to the tags
 * 
 * + Check flags [false]: bad images are processed correctly if format check is disabled
 * Check flags [false]: images with wrong hash in the name are processed correctly if hash check is disabled
 * Check flags [false]: images without hash are processed correctly if no hash check is disabled
 * Check flags [false]: images don't push its name to the tags if the corresponding flag is disabled
 * Check flags [false]: images don't push its folder name to the tags if the corresponding flag is disabled
 * 
 * Formats: all files are processed if formats are empty
 * Formats: only files with the specified extensions are processed if formats are specified
 * 
 * # Destination folder options
 * Check flags: files are renamed if the corresponding flag is enabled
 * Check flags: files are moved to the hash subfolder if the corresponding flag is enabled
 * Check flags [false]: files are not renamed if the corresponding flag is disabled
 * Check flags [false]: files are not moved to the hash subfolder if the corresponding flag is disabled
 * 
 */

[Collection("WebApplication")]
public class CollectionFilesTests
{
    private readonly TestWebApplicationFactory<Program> _webApp;

    public CollectionFilesTests(TestWebApplicationFactory<Program> webApp) => _webApp = webApp;

    [Fact]
    public async Task FileInSourceFolderShouldBeAddedToDatabaseAndMovedToDestinationFolder()
    {
        // arrange
        var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var httpClient = _webApp.Client;

        var collectionId = await CreateCollection(httpClient);
        var collectionPath = Guid.NewGuid().ToString();
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "source");
        var destFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "dest");
        
        Directory.CreateDirectory(sourceFolderPath);
        Directory.CreateDirectory(destFolderPath);
        
        var addSourceFolderCommand = new AddSourceFolderCommand(collectionId, sourceFolderPath, true, true, true, true, new[] { "jpg" });
        var addDestinationFolderCommand = new SetDestinationFolderCommand(collectionId, destFolderPath, true, true, "!format-error", "!hash-error", "!no-hash-error");
        
        await httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        await httpClient.PostAsJsonAsync("/collections/destination-folder", addDestinationFolderCommand);
        
        // act
        var file = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        file.CopyTo(Path.Combine(sourceFolderPath, file.Name));
        await mediator.Send(new OverseeCommand(false));
        
        // assert
        var savedFile = await context.CollectionFiles.FirstOrDefaultAsync(x => x.CollectionId == collectionId);

        savedFile.Should().NotBeNull();
        savedFile!.Path.Should().Be(Path.Combine(destFolderPath, "5f", "30", "5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        savedFile.Md5.Should().Be("5f30f9953332c230d11e3f26db5ae9a0");
        savedFile.Size.Should().Be(150881);
        savedFile.IsRemoved.Should().BeFalse();
        savedFile.OriginalPath.Should().Be(Path.Combine(sourceFolderPath, file.Name));

        harness.Sent.Count().Should().BeGreaterOrEqualTo(2);

        var newFileCommands = harness.Sent.Select(x => x.MessageType == typeof(INewFileCommand)).ToList();

        newFileCommands.Any(x =>
                x.MessageObject is INewFileCommand message
                && message.FileId == savedFile.Id
                && message.Md5 == "5f30f9953332c230d11e3f26db5ae9a0")
            .Should().BeTrue();

        var updateMetadataCommands = harness.Sent.Select(x => x.MessageType == typeof(IUpdateMetadataCommand)).ToList();
        
        updateMetadataCommands.Any(x =>
                x.MessageObject is IUpdateMetadataCommand message
                && message.FileId == savedFile.Id
                && message.MetadataSource == MetadataSource.Manual
                && message.FileTags[0].Type == "Location"
                && message.FileTags[0].Name == "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg")
            .Should().BeTrue();
    }

    [Fact]
    public async Task FileInSourceFolderWithoutDestinationShouldBeAddedToDatabaseAndNotMoved()
    {
        // arrange
        var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var httpClient = _webApp.Client;

        var collectionId = await CreateCollection(httpClient);
        var collectionPath = Guid.NewGuid().ToString();
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collections", collectionPath, "source");
        var innerSourceFolderPath = Path.Combine(sourceFolderPath, "inner");
        Directory.CreateDirectory(innerSourceFolderPath);
        
        var addSourceFolderCommand = new AddSourceFolderCommand(collectionId, sourceFolderPath, true, true, true, true, new[] { "jpg" });
        await httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        
        // act
        var file = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFilePath = Path.Combine(innerSourceFolderPath, file.Name);
        file.CopyTo(testFilePath);
        
        await mediator.Send(new OverseeCommand(false));
        
        // assert
        var savedFile = await context.CollectionFiles.FirstOrDefaultAsync(x => x.CollectionId == collectionId);

        savedFile.Should().NotBeNull();
        savedFile!.Path.Should().Be(testFilePath);
        savedFile.Md5.Should().Be("5f30f9953332c230d11e3f26db5ae9a0");
        savedFile.Size.Should().Be(150881);
        savedFile.IsRemoved.Should().BeFalse();
        savedFile.OriginalPath.Should().Be(testFilePath);

        harness.Sent.Count().Should().BeGreaterOrEqualTo(2);

        var newFileCommands = harness.Sent.Select(x => x.MessageType == typeof(INewFileCommand)).ToList();

        newFileCommands.Any(x =>
                x.MessageObject is INewFileCommand message
                && message.FileId == savedFile.Id
                && message.Md5 == "5f30f9953332c230d11e3f26db5ae9a0")
            .Should().BeTrue();

        var updateMetadataCommands = harness.Sent.Select(x => x.MessageType == typeof(IUpdateMetadataCommand)).ToList();
        
        updateMetadataCommands.Any(x =>
                x.MessageObject is IUpdateMetadataCommand message
                && message.FileId == savedFile.Id
                && message.MetadataSource == MetadataSource.Manual
                && message.FileTags[0].Type == "Location"
                && message.FileTags[0].Name == "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"
                && message.FileTags[1].Type == "Location"
                && message.FileTags[1].Name == "inner")
            .Should().BeTrue();
    }

    [Fact]
    public async Task DuplicateImagesAreDeletedFromSourceFolder()
    {
        // arrange
        var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var httpClient = _webApp.Client;

        var collectionId = await CreateCollection(httpClient);
        var collectionPath = Guid.NewGuid().ToString();
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "source");
        var destFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "dest");
        
        Directory.CreateDirectory(sourceFolderPath);
        Directory.CreateDirectory(destFolderPath);
        
        var addSourceFolderCommand = new AddSourceFolderCommand(collectionId, sourceFolderPath, true, true, true, true, new[] { "jpg" });
        var addDestinationFolderCommand = new SetDestinationFolderCommand(collectionId, destFolderPath, true, true, "!format-error", "!hash-error", "!no-hash-error");
        
        await httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        await httpClient.PostAsJsonAsync("/collections/destination-folder", addDestinationFolderCommand);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await mediator.Send(new OverseeCommand(false));

        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await mediator.Send(new OverseeCommand(false));
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
    }

    [Fact]
    public async Task ImagesWithSameMd5AreDeletedFromSourceFolder()
    {
        // arrange
        var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var httpClient = _webApp.Client;

        var collectionId = await CreateCollection(httpClient);
        var collectionPath = Guid.NewGuid().ToString();
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "source");
        var destFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "dest");
        
        Directory.CreateDirectory(sourceFolderPath);
        Directory.CreateDirectory(destFolderPath);
        
        var addSourceFolderCommand = new AddSourceFolderCommand(collectionId, sourceFolderPath, true, true, true, true, new[] { "jpg" });
        var addDestinationFolderCommand = new SetDestinationFolderCommand(collectionId, destFolderPath, true, true, "!format-error", "!hash-error", "!no-hash-error");
        
        await httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        await httpClient.PostAsJsonAsync("/collections/destination-folder", addDestinationFolderCommand);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await mediator.Send(new OverseeCommand(false));

        testFile.CopyTo(Path.Combine(sourceFolderPath, "diff-name" + testFile.Name));
        await mediator.Send(new OverseeCommand(false));
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
    }

    [Fact]
    public async Task ImagesWithSameNameButDifferentMd5AreProcessedCorrectly()
    {
        // arrange
        var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var httpClient = _webApp.Client;

        var collectionId = await CreateCollection(httpClient);
        var collectionPath = Guid.NewGuid().ToString();
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "source");
        var destFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "dest");
        
        Directory.CreateDirectory(sourceFolderPath);
        Directory.CreateDirectory(destFolderPath);
        
        var addSourceFolderCommand = new AddSourceFolderCommand(collectionId, sourceFolderPath, true, false, true, true, new[] { "jpg" });
        var addDestinationFolderCommand = new SetDestinationFolderCommand(collectionId, destFolderPath, true, true, "!format-error", "!hash-error", "!no-hash-error");
        
        await httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        await httpClient.PostAsJsonAsync("/collections/destination-folder", addDestinationFolderCommand);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        
        // act
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        await mediator.Send(new OverseeCommand(false));

        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        await mediator.Send(new OverseeCommand(false));
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(2);
    }

    [Fact]
    public async Task BrokenImagesAreMovedToFormatErrorFolder()
    {
        // arrange
        var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var httpClient = _webApp.Client;

        var collectionId = await CreateCollection(httpClient);
        var collectionPath = Guid.NewGuid().ToString();
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "source");
        var destFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "dest");
        
        Directory.CreateDirectory(sourceFolderPath);
        Directory.CreateDirectory(destFolderPath);
        
        var addSourceFolderCommand = new AddSourceFolderCommand(collectionId, sourceFolderPath, true, false, true, true, new[] { "jpg" });
        var addDestinationFolderCommand = new SetDestinationFolderCommand(collectionId, destFolderPath, true, true, "!format-error", "!hash-error", "!no-hash-error");
        
        await httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        await httpClient.PostAsJsonAsync("/collections/destination-folder", addDestinationFolderCommand);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "empty-file.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await mediator.Send(new OverseeCommand(false));
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(0);
        Directory.GetFiles(Path.Combine(destFolderPath, "!format-error")).Should().NotBeEmpty();
    }

    [Fact]
    public async Task BrokenImagesAreNotMovedToFormatErrorFolderWhenFormatCheckIsDisabled()
    {
        // arrange
        var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var httpClient = _webApp.Client;

        var collectionId = await CreateCollection(httpClient);
        var collectionPath = Guid.NewGuid().ToString();
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "source");
        var destFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "dest");
        
        Directory.CreateDirectory(sourceFolderPath);
        Directory.CreateDirectory(destFolderPath);
        
        var addSourceFolderCommand = new AddSourceFolderCommand(collectionId, sourceFolderPath, false, false, true, true, new[] { "jpg" });
        var addDestinationFolderCommand = new SetDestinationFolderCommand(collectionId, destFolderPath, true, true, "!format-error", "!hash-error", "!no-hash-error");
        
        await httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        await httpClient.PostAsJsonAsync("/collections/destination-folder", addDestinationFolderCommand);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "empty-file.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await mediator.Send(new OverseeCommand(false));
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
        Directory.Exists(Path.Combine(destFolderPath, "!format-error")).Should().BeFalse();
    }

    private static async Task<Guid> CreateCollection(HttpClient httpClient)
    {
        var collectionName = $"Collection Name {Guid.NewGuid()}";
        var result = await httpClient.PostAsJsonAsync("/collections", new {name = collectionName});
        var resultString = await result.Content.ReadAsStringAsync();
        return Guid.Parse(resultString[1..^1]);
    }
}
