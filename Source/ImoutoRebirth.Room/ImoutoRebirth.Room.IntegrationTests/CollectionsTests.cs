using System.Net.Http.Json;
using FluentAssertions;
using ImoutoRebirth.Room.IntegrationTests.Fixtures;
using ImoutoRebirth.Room.UI.WebApi;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.IntegrationTests;

[Collection("WebApplication")]
public class CollectionsTests
{
    private readonly TestWebApplicationFactory<Program> _webApp;

    public CollectionsTests(TestWebApplicationFactory<Program> webApp) => _webApp = webApp;

    [Fact]
    public async Task CreateCollection()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;

        var randomName = $"collection name {Guid.NewGuid()}";
        
        // act
        var result = await httpClient.PostAsJsonAsync("/collections", new {name = randomName});
        
        // assert
        var resultString = await result.Content.ReadAsStringAsync();
        var createdCollectionId = Guid.Parse(resultString[1..^1]);

        var collections = await context.Collections.ToListAsync();
        var collection = collections.FirstOrDefault(x => x.Name == randomName);

        collection.Should().NotBeNull();
        collection!.Id.Should().Be(createdCollectionId);
        collection.Name.Should().Be(randomName);
    }
    
    [Fact]
    public async Task RenameCollection()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;

        var randomName = $"collection name {Guid.NewGuid()}";
        var randomName2 = $"collection name 2 {Guid.NewGuid()}";
        
        var creationResult = await httpClient.PostAsJsonAsync("/collections", new {name = randomName});
        var creationResultString = await creationResult.Content.ReadAsStringAsync();
        var createdCollectionId = Guid.Parse(creationResultString[1..^1]);
        
        // act
        await httpClient.PatchAsync($"/collections/{createdCollectionId}?newName={randomName2}", null);
        
        // assert
        var collections = await context.Collections.ToListAsync();
        var collection = collections.SingleOrDefault(x => x.Name == randomName2);

        collection.Should().NotBeNull();
        collection!.Id.Should().Be(createdCollectionId);
        collection.Name.Should().Be(randomName2);
    }
    
    [Fact]
    public async Task DeleteCollection()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;

        var randomName = $"collection name {Guid.NewGuid()}";
        
        var creationResult = await httpClient.PostAsJsonAsync("/collections", new {name = randomName});
        var creationResultString = await creationResult.Content.ReadAsStringAsync();
        var createdCollectionId = Guid.Parse(creationResultString[1..^1]);
        
        // act
        await httpClient.DeleteAsync($"/collections/{createdCollectionId}");
        
        // assert
        var collections = await context.Collections.ToListAsync();
        var collectionByName = collections.SingleOrDefault(x => x.Name == randomName);
        var collectionById = collections.SingleOrDefault(x => x.Id == createdCollectionId);

        collectionByName.Should().BeNull();
        collectionById.Should().BeNull();
    }
    
    [Fact]
    public async Task GetAllCollection()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;

        var randomName1 = $"collection name {Guid.NewGuid()}";
        var randomName2 = $"collection name {Guid.NewGuid()}";
        var randomName3 = $"collection name {Guid.NewGuid()}";
        
        await httpClient.PostAsJsonAsync("/collections", new {name = randomName1});
        await httpClient.PostAsJsonAsync("/collections", new {name = randomName2});
        await httpClient.PostAsJsonAsync("/collections", new {name = randomName3});
        
        // act
        var collections = await httpClient.GetFromJsonAsync<IReadOnlyCollection<CollectionResponse>>($"/collections");
        
        // assert
        var dbCollections = await context.Collections.ToListAsync();

        collections.Should().NotBeNull();
        collections!.Count.Should().Be(dbCollections.Count);
        collections.FirstOrDefault(x => x.Name == randomName1).Should().NotBeNull();
        collections.FirstOrDefault(x => x.Name == randomName2).Should().NotBeNull();
        collections.FirstOrDefault(x => x.Name == randomName3).Should().NotBeNull();
    }
}
