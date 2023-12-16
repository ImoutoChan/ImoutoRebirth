using System.Net.Http.Json;
using System.Reflection;
using FluentAssertions;
using ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;
using ImoutoRebirth.Room.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ImoutoRebirth.Room.IntegrationTests;

[Collection("WebApplication")]
public class SourceFoldersTests
{
    private readonly TestWebApplicationFactory<Program> _webApp;

    public SourceFoldersTests(TestWebApplicationFactory<Program> webApp) => _webApp = webApp;

    [Fact]
    public async Task CreateSourceFolder()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;

        var collectionId = await CreateCollection(httpClient);
        
        var location = Assembly.GetExecutingAssembly().Location;
        
        // act
        var command = new AddSourceFolderCommand(
            collectionId,
            Path.Combine(location, "collection", "source"),
            true,
            true,
            true,
            true,
            new[] {".jpg", ".png", ".gif", ".webp", ".bmp", ".jpeg", ".tiff", ".tif", ".jfif"});
        
        var result =await httpClient.PostAsJsonAsync("/collections/source-folders", command);
        var resultString = await result.Content.ReadAsStringAsync();
        var sourceFolderId = Guid.Parse(resultString[1..^1]);

        // assert
        var sourceFolder = await context.SourceFolders.FirstOrDefaultAsync(x => x.CollectionId == collectionId);

        sourceFolder.Should().NotBeNull();
        sourceFolder!.CollectionId.Should().Be(command.CollectionId);
        sourceFolder.Id.Should().Be(sourceFolderId);
        sourceFolder.Path.Should().Be(command.Path);
        sourceFolder.ShouldCheckFormat.Should().Be(command.ShouldCheckFormat);
        sourceFolder.ShouldAddTagFromFilename.Should().Be(command.ShouldAddTagFromFilename);
        sourceFolder.ShouldCheckHashFromName.Should().Be(command.ShouldCheckHashFromName);
        sourceFolder.ShouldCreateTagsFromSubfolders.Should().Be(command.ShouldCreateTagsFromSubfolders);
        sourceFolder.SupportedExtensionCollection.Should().BeEquivalentTo(command.SupportedExtensions);
    }

    [Fact]
    public async Task UpdateSourceFolder()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;
    
        var collectionId = await CreateCollection(httpClient);
        
        var location = Assembly.GetExecutingAssembly().Location;
        var command = new AddSourceFolderCommand(
            collectionId,
            Path.Combine(location, "collection", "source"),
            true,
            true,
            true,
            true,
            new[] {".jpg", ".png", ".gif", ".webp", ".bmp", ".jpeg", ".tiff", ".tif", ".jfif"});
        
        var result =await httpClient.PostAsJsonAsync("/collections/source-folders", command);
        var resultString = await result.Content.ReadAsStringAsync();
        var sourceFolderId = Guid.Parse(resultString[1..^1]);
        
        // act
        var updateCommand = new UpdateSourceFolderCommand(
            collectionId,
            sourceFolderId,
            Path.Combine(location, "collection", "source 1"),
            false,
            false,
            false,
            false,
            new[] {".jfif"});
        
        await httpClient.PutAsJsonAsync("/collections/source-folders", updateCommand);
        
        // assert
        var sourceFolder = await context.SourceFolders.SingleOrDefaultAsync(x => x.Id == sourceFolderId);
    
        sourceFolder.Should().NotBeNull();
        sourceFolder!.CollectionId.Should().Be(updateCommand.CollectionId);
        sourceFolder.Id.Should().Be(sourceFolderId);
        sourceFolder.Path.Should().Be(updateCommand.Path);
        sourceFolder.ShouldCheckFormat.Should().Be(updateCommand.ShouldCheckFormat);
        sourceFolder.ShouldAddTagFromFilename.Should().Be(updateCommand.ShouldAddTagFromFilename);
        sourceFolder.ShouldCheckHashFromName.Should().Be(updateCommand.ShouldCheckHashFromName);
        sourceFolder.ShouldCreateTagsFromSubfolders.Should().Be(updateCommand.ShouldCreateTagsFromSubfolders);
        sourceFolder.SupportedExtensionCollection.Should().BeEquivalentTo(updateCommand.SupportedExtensions);
    }

    [Fact]
    public async Task DeleteSourceFolder()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;
    
        var collectionId = await CreateCollection(httpClient);
        
        var location = Assembly.GetExecutingAssembly().Location;
        var command = new AddSourceFolderCommand(
            collectionId,
            Path.Combine(location, "collection", "source"),
            true,
            true,
            true,
            true,
            new[] {".jpg", ".png", ".gif", ".webp", ".bmp", ".jpeg", ".tiff", ".tif", ".jfif"});
        
        var result =await httpClient.PostAsJsonAsync("/collections/source-folders", command);
        var resultString = await result.Content.ReadAsStringAsync();
        var sourceFolderId = Guid.Parse(resultString[1..^1]);
        
        // act
        await httpClient.DeleteAsync($"/collections/{collectionId}/source-folders/{sourceFolderId}");
        
        // assert
        var sourceFolder = await context.SourceFolders.FirstOrDefaultAsync(x => x.Id == sourceFolderId);
    
        sourceFolder.Should().BeNull();
    }

    [Fact]
    public async Task GetSourceFolders()
    {
        // arrange
        var httpClient = _webApp.Client;
    
        var collectionId = await CreateCollection(httpClient);
        
        var location = Assembly.GetExecutingAssembly().Location;

        var command1 = new AddSourceFolderCommand(collectionId, Path.Combine(location, "collection", "source 1"), true, true, true, true, new[] {".jpg", ".png", ".gif", ".webp", ".bmp", ".jpeg", ".tiff", ".tif", ".jfif"});
        var command2 = command1 with { Path = Path.Combine(location, "collection", "source 2") };
        var command3 = command1 with { Path = Path.Combine(location, "collection", "source 3") };
        var command4 = command1 with { Path = Path.Combine(location, "collection", "source 4") };
        
        await httpClient.PostAsJsonAsync("/collections/source-folders", command1);
        await httpClient.PostAsJsonAsync("/collections/source-folders", command2);
        await httpClient.PostAsJsonAsync("/collections/source-folders", command3);
        await httpClient.PostAsJsonAsync("/collections/source-folders", command4);
        
        // act
    
        var sourceFolders = await httpClient
            .GetFromJsonAsync<SourceFolderInfo[]>($"/collections/{collectionId}/source-folders");
        
        // assert
        sourceFolders.Should().NotBeNull();
        sourceFolders.Should().HaveCount(4);
        sourceFolders!.Select(x => x.Path).Should()
            .BeEquivalentTo([
                command1.Path, 
                command2.Path, 
                command3.Path, 
                command4.Path
            ]);
    }

    private static async Task<Guid> CreateCollection(HttpClient httpClient)
    {
        var collectionName = $"Collection Name {Guid.NewGuid()}";
        var result = await httpClient.PostAsJsonAsync("/collections", new {name = collectionName});
        var resultString = await result.Content.ReadAsStringAsync();
        return Guid.Parse(resultString[1..^1]);
    }
}
