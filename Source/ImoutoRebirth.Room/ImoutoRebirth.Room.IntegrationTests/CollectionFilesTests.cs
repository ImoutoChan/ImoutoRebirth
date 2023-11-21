using System.Net.Http.Json;
using System.Reflection;
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
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", "source");
        var destFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", "dest");

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

        harness.Sent.Count().Should().Be(2);

        var newFileCommand = harness.Sent.Select(x => x.MessageType == typeof(INewFileCommand)).FirstOrDefault();
        newFileCommand.Should().NotBeNull();
        var message1 = newFileCommand!.MessageObject as INewFileCommand;
        message1!.Md5.Should().Be("5f30f9953332c230d11e3f26db5ae9a0");
        message1.FileId.Should().Be(savedFile.Id);

        var updateMetadataCommand = harness.Sent.Select(x => x.MessageType == typeof(IUpdateMetadataCommand)).FirstOrDefault();
        updateMetadataCommand.Should().NotBeNull();
        var message2 = updateMetadataCommand!.MessageObject as IUpdateMetadataCommand;
        message2!.FileId.Should().Be(savedFile.Id);
        message2.MetadataSource.Should().Be(MetadataSource.Manual);
        message2.FileTags[0].Type.Should().Be("Location");
        message2.FileTags[0].Name.Should().Be("file1-5f30f9953332c230d11e3f26db5ae9a0.jpg");
    }

    private static async Task<Guid> CreateCollection(HttpClient httpClient)
    {
        var collectionName = $"Collection Name {Guid.NewGuid()}";
        var result = await httpClient.PostAsJsonAsync("/collections", new {name = collectionName});
        var resultString = await result.Content.ReadAsStringAsync();
        return Guid.Parse(resultString[1..^1]);
    }
}
