using System.Net.Http.Json;
using System.Reflection;
using FluentAssertions;
using ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ImoutoRebirth.Room.IntegrationTests;

[Collection("WebApplication")]
public class DestinationFoldersTests
{
    private readonly TestWebApplicationFactory<Program> _webApp;

    public DestinationFoldersTests(TestWebApplicationFactory<Program> webApp)
    {
        _webApp = webApp;
    }

    [Fact]
    public async Task CreateDestinationFolder()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;

        var collectionName = $"Collection Name {Guid.NewGuid()}";
        var result = await httpClient.PostAsJsonAsync("/collections", new {name = collectionName});
        var resultString = await result.Content.ReadAsStringAsync();
        var createdCollectionId = Guid.Parse(resultString[1..^1]);
        
        var location = Assembly.GetExecutingAssembly().Location;
        
        // act
        var command = new
        {
            CollectionId = createdCollectionId,
            Path = Path.Combine(location, "collection", "destination"),
            ShouldCreateSubfoldersByHash = true,
            ShouldRenameByHash = true,
            FormatErrorSubfolder = "format-error-subfolder",
            HashErrorSubfolder = "hash-error-subfolder",
            WithoutHashErrorSubfolder = "no-hash-error-subfolder",
        };
        
        await httpClient.PostAsJsonAsync("/collections/destination-folder", command);

        // assert
        var destinationFolder =
            await context.DestinationFolders
                .FirstOrDefaultAsync(x => x.CollectionId == createdCollectionId);

        destinationFolder.Should().NotBeNull();
        destinationFolder!.CollectionId.Should().Be(command.CollectionId);
        destinationFolder.Path.Should().Be(command.Path);
        destinationFolder.ShouldCreateSubfoldersByHash.Should().Be(command.ShouldCreateSubfoldersByHash);
        destinationFolder.ShouldRenameByHash.Should().Be(command.ShouldRenameByHash);
        destinationFolder.FormatErrorSubfolder.Should().Be(command.FormatErrorSubfolder);
        destinationFolder.HashErrorSubfolder.Should().Be(command.HashErrorSubfolder);
        destinationFolder.WithoutHashErrorSubfolder.Should().Be(command.WithoutHashErrorSubfolder);
    }

    [Fact]
    public async Task UpdateDestinationFolder()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;

        var collectionName = $"Collection Name {Guid.NewGuid()}";
        var result = await httpClient.PostAsJsonAsync("/collections", new {name = collectionName});
        var resultString = await result.Content.ReadAsStringAsync();
        var createdCollectionId = Guid.Parse(resultString[1..^1]);
        
        var location = Assembly.GetExecutingAssembly().Location;
        var command = new
        {
            CollectionId = createdCollectionId,
            Path = Path.Combine(location, "collection", "destination"),
            ShouldCreateSubfoldersByHash = true,
            ShouldRenameByHash = true,
            FormatErrorSubfolder = "format-error-subfolder",
            HashErrorSubfolder = "hash-error-subfolder",
            WithoutHashErrorSubfolder = "no-hash-error-subfolder",
        };
        
        await httpClient.PostAsJsonAsync("/collections/destination-folder", command);
        
        // act
        var updateCommand = new
        {
            CollectionId = createdCollectionId,
            Path = Path.Combine(location, "collection1", "destination1"),
            ShouldCreateSubfoldersByHash = false,
            ShouldRenameByHash = false,
            FormatErrorSubfolder = "format-error-subfolder 1",
            HashErrorSubfolder = "hash-error-subfolder 1",
            WithoutHashErrorSubfolder = "no-hash-error-subfolder 1",
        };
        
        await httpClient.PostAsJsonAsync("/collections/destination-folder", updateCommand);
        
        // assert
        var destinationFolder =
            await context.DestinationFolders
                .SingleOrDefaultAsync(x => x.CollectionId == createdCollectionId);

        destinationFolder.Should().NotBeNull();
        destinationFolder!.CollectionId.Should().Be(updateCommand.CollectionId);
        destinationFolder.Path.Should().Be(updateCommand.Path);
        destinationFolder.ShouldCreateSubfoldersByHash.Should().Be(updateCommand.ShouldCreateSubfoldersByHash);
        destinationFolder.ShouldRenameByHash.Should().Be(updateCommand.ShouldRenameByHash);
        destinationFolder.FormatErrorSubfolder.Should().Be(updateCommand.FormatErrorSubfolder);
        destinationFolder.HashErrorSubfolder.Should().Be(updateCommand.HashErrorSubfolder);
        destinationFolder.WithoutHashErrorSubfolder.Should().Be(updateCommand.WithoutHashErrorSubfolder);
    }

    [Fact]
    public async Task DeleteDestinationFolder()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;

        var collectionName = $"Collection Name {Guid.NewGuid()}";
        var result = await httpClient.PostAsJsonAsync("/collections", new {name = collectionName});
        var resultString = await result.Content.ReadAsStringAsync();
        var createdCollectionId = Guid.Parse(resultString[1..^1]);
        
        var location = Assembly.GetExecutingAssembly().Location;
        var command = new
        {
            CollectionId = createdCollectionId,
            Path = Path.Combine(location, "collection", "destination"),
            ShouldCreateSubfoldersByHash = true,
            ShouldRenameByHash = true,
            FormatErrorSubfolder = "format-error-subfolder",
            HashErrorSubfolder = "hash-error-subfolder",
            WithoutHashErrorSubfolder = "no-hash-error-subfolder",
        };
        
        await httpClient.PostAsJsonAsync("/collections/destination-folder", command);
        
        // act
        
        await httpClient.DeleteAsync($"/collections/{createdCollectionId}/destination-folder");
        
        // assert
        var destinationFolder =
            await context.DestinationFolders
                .SingleOrDefaultAsync(x => x.CollectionId == createdCollectionId);

        destinationFolder.Should().BeNull();
    }

    [Fact]
    public async Task GetDestinationFolder()
    {
        // arrange
        var httpClient = _webApp.Client;

        var collectionName = $"Collection Name {Guid.NewGuid()}";
        var result = await httpClient.PostAsJsonAsync("/collections", new {name = collectionName});
        var resultString = await result.Content.ReadAsStringAsync();
        var createdCollectionId = Guid.Parse(resultString[1..^1]);
        
        var location = Assembly.GetExecutingAssembly().Location;
        var command = new
        {
            CollectionId = createdCollectionId,
            Path = Path.Combine(location, "collection", "destination"),
            ShouldCreateSubfoldersByHash = true,
            ShouldRenameByHash = true,
            FormatErrorSubfolder = "format-error-subfolder",
            HashErrorSubfolder = "hash-error-subfolder",
            WithoutHashErrorSubfolder = "no-hash-error-subfolder",
        };
        
        await httpClient.PostAsJsonAsync("/collections/destination-folder", command);
        
        // act

        var destinationFolder =
            await httpClient.GetFromJsonAsync<DestinationFolderInfo>(
                $"/collections/{createdCollectionId}/destination-folder");
        
        // assert
        destinationFolder.Should().NotBeNull();
        destinationFolder!.CollectionId.Should().Be(command.CollectionId);
        destinationFolder.Path.Should().Be(command.Path);
        destinationFolder.ShouldCreateSubfoldersByHash.Should().Be(command.ShouldCreateSubfoldersByHash);
        destinationFolder.ShouldRenameByHash.Should().Be(command.ShouldRenameByHash);
        destinationFolder.FormatErrorSubfolder.Should().Be(command.FormatErrorSubfolder);
        destinationFolder.HashErrorSubfolder.Should().Be(command.HashErrorSubfolder);
        destinationFolder.WithoutHashErrorSubfolder.Should().Be(command.WithoutHashErrorSubfolder);
    }
}