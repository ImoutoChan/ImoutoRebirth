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

        if (Directory.Exists(sourceFolderPath))
            Directory.Delete(sourceFolderPath, true);
        
        if (Directory.Exists(destFolderPath))
            Directory.Delete(destFolderPath, true);
        
        Directory.CreateDirectory(sourceFolderPath);
        Directory.CreateDirectory(destFolderPath);
        
        var addSourceFolderCommand = new AddSourceFolderCommand(collectionId, sourceFolderPath, true, true, true, true, new[] { "jpg" });
        var addDestinationFolderCommand = new SetDestinationFolderCommand(collectionId, destFolderPath, true, true, "!format-error", "!hash-error", "!no-hash-error");
        
        await httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        await httpClient.PostAsJsonAsync("/collections/destination-folder", addDestinationFolderCommand);
        
        // act
        var file = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        file.CopyTo(Path.Combine(sourceFolderPath, file.Name));
        await mediator.Send(new OverseeCommand());
        
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
        
        await mediator.Send(new OverseeCommand());
        
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
                && message.FileTags[1].Name == "inner"
                )
            .Should().BeTrue();
    }

    private static async Task<Guid> CreateCollection(HttpClient httpClient)
    {
        var collectionName = $"Collection Name {Guid.NewGuid()}";
        var result = await httpClient.PostAsJsonAsync("/collections", new {name = collectionName});
        var resultString = await result.Content.ReadAsStringAsync();
        return Guid.Parse(resultString[1..^1]);
    }
}
