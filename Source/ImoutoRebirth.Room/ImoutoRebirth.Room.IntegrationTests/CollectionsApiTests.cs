using System.Net.Http.Json;
using FluentAssertions;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Room.Application.Cqrs;
using ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;
using ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Domain;
using ImoutoRebirth.Room.IntegrationTests.Fixtures;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ImoutoRebirth.Room.IntegrationTests;

[Collection("WebApplication")]
public class CollectionsApiTests : IDisposable
{
    private readonly TestWebApplicationFactory<Program> _webApp;
    private readonly IServiceScope _scope;
    private readonly RoomDbContext _context;
    private readonly IMediator _mediator;
    private readonly ITestHarness _harness;
    private readonly HttpClient _httpClient;

    public CollectionsApiTests(TestWebApplicationFactory<Program> webApp)
    {
        _webApp = webApp;

        _scope = _webApp.GetScope();
        _context = _webApp.GetDbContext(_scope);
        _mediator = _scope.ServiceProvider.GetRequiredService<IMediator>();
        _harness = _scope.ServiceProvider.GetRequiredService<ITestHarness>();
        _httpClient = _webApp.Client;
    }

    public void Dispose()
    {
        _scope.Dispose();
        _context.Dispose();
        _httpClient.Dispose();
    }

    [Fact]
    public async Task GetCollectionFiles()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        await _mediator.Send(new OverseeCommand(false));
        
        // act
        var response = await _httpClient.PostAsJsonAsync(
            "/collection-files",
            new CollectionFilesQuery(
                collectionId,
                Array.Empty<Guid>(),
                null,
                null,
                5,
                0));

        var files = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<CollectionFile>>();
        
        // assert
        files.Should().HaveCount(2);
        files!.First().Md5.Should().Be("5f30f9953332c230d11e3f26db5ae9a0");
        files!.First().Size.Should().Be(150881);
        files!.First().Path.Should().Be(Path.Combine(destFolderPath, "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        files!.ElementAt(1).Md5.Should().Be("09e56a8fd9d1e8beb62c50e6945632bf");
        files!.ElementAt(1).Size.Should().Be(830998);
        files!.ElementAt(1).Path.Should().Be(Path.Combine(destFolderPath, "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
    }

    [Fact]
    public async Task FilterCollectionFiles()
    {
        // arrange
        var (_, sourceFolderPath, _) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        await _mediator.Send(new OverseeCommand(false));
        
        // act
        var response = await _httpClient.PostAsJsonAsync(
            "/collection-files/filter-hashes",
            new FilterCollectionFileHashesQuery(
                new[]
                {
                    "5f30f9953332c230d11e3f26db5ae9a0", 
                    "09e56a8fd9d1e8beb62c50e6945632bf",
                    "12356a8fd9d1e8beb62c50e6945632bf"
                }));

        var files = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<string>>();
        
        // assert
        files.Should().HaveCount(2);
        files.Should().Contain("5f30f9953332c230d11e3f26db5ae9a0");
        files.Should().Contain("09e56a8fd9d1e8beb62c50e6945632bf");
    }

    [Fact]
    public async Task GetCollectionFilesIds()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        await _mediator.Send(new OverseeCommand(false));
        
        // act
        var response = await _httpClient.PostAsJsonAsync(
            "/collection-files/search-ids",
            new CollectionFilesQuery(
                collectionId,
                Array.Empty<Guid>(),
                null,
                null,
                5,
                0));

        var files = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<Guid>>();
        
        // assert
        files.Should().HaveCount(2);
    }

    [Fact]
    public async Task CountCollectionFiles()
    {
        // arrange
        var (collectionId, sourceFolderPath, _) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        await _mediator.Send(new OverseeCommand(false));
        
        // act
        var response = await _httpClient.PostAsJsonAsync(
            "/collection-files/count",
            new CollectionFilesQuery(
                collectionId,
                Array.Empty<Guid>(),
                null,
                null,
                5,
                0));

        var count = await response.Content.ReadFromJsonAsync<int>();
        
        // assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task DeleteCollectionFile()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        await _mediator.Send(new OverseeCommand(false));

        var fileIds = _context.CollectionFiles
            .Where(x => x.CollectionId == collectionId)
            .Select(x => x.Id)
            .ToList();
        
        // act
        var response = await _httpClient.DeleteAsync($"/collection-files/{fileIds.First()}");
        response.EnsureSuccessStatusCode();
        
        // assert
        var countResponse = await _httpClient.PostAsJsonAsync(
            "/collection-files/count",
            new CollectionFilesQuery(
                collectionId,
                Array.Empty<Guid>(),
                null,
                null,
                5,
                0));
        var count = await countResponse.Content.ReadFromJsonAsync<int>();
        count.Should().Be(1);

        File.Exists(Path.Combine(destFolderPath, testFile2.Name)).Should().BeFalse();
        File.Exists(Path.Combine(destFolderPath, "!Deleted", testFile2.Name)).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateLocationTags()
    {
        // arrange
        var collectionId = await CreateCollection();
        var collectionPath = Guid.NewGuid().ToString();
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "source");
        
        Directory.CreateDirectory(sourceFolderPath);
        
        var addSourceFolderCommand = new AddSourceFolderCommand(collectionId, sourceFolderPath, false, false, true, true, new []{ "jpg" });
        
        await _httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));

        Directory.CreateDirectory(Path.Combine(sourceFolderPath, "inner"));
        testFile1.CopyTo(Path.Combine(sourceFolderPath, "inner", testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, "inner", testFile2.Name));
        await _mediator.Send(new OverseeCommand(false));
        
        // act
        await _httpClient.PostAsync("/collection-files/updateSourceTags", null);

        var files = await _context.CollectionFiles.Where(x => x.CollectionId == collectionId).ToListAsync();
        
        // assert
        _harness.Sent.SelectMessages<IUpdateMetadataCommand>().Any(x =>
            x.MetadataSource == MetadataSource.Manual
            && x.FileId == files[0].Id || x.FileId == files[1].Id
            && x.FileTags.Any(y => y is { Name: "inner", Type: "Location" })
            && x.FileTags.Any(y => y is { Name: "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg", Type: "Location" })
            ).Should().BeTrue();
        _harness.Sent.SelectMessages<IUpdateMetadataCommand>().Any(x =>
            x.MetadataSource == MetadataSource.Manual
            && x.FileId == files[0].Id || x.FileId == files[1].Id
            && x.FileTags.Any(y => y is { Name: "inner", Type: "Location" })
            && x.FileTags.Any(y => y is { Name: "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg", Type: "Location" })
            ).Should().BeTrue();
    }

    private async Task<CreatedCollection> CreateDefaultCollection(
        bool sourceShouldCheckFormat = false,
        bool sourceShouldCheckHashFromName = false,
        bool sourceShouldCreateTagsFromSubfolders = false,
        bool sourceShouldAddTagFromFilename = false,
        IReadOnlyCollection<string>? sourceSupportedExtensions = null,
        bool destShouldCreateSubfoldersByHash = false,
        bool destShouldRenameByHash = false)
    {
        sourceSupportedExtensions ??= Array.Empty<string>();
        
        var collectionId = await CreateCollection();
        var collectionPath = Guid.NewGuid().ToString();
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "source");
        var destFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "dest");
        
        Directory.CreateDirectory(sourceFolderPath);
        Directory.CreateDirectory(destFolderPath);
        
        var addSourceFolderCommand = new AddSourceFolderCommand(collectionId, sourceFolderPath, sourceShouldCheckFormat, sourceShouldCheckHashFromName, sourceShouldCreateTagsFromSubfolders, sourceShouldAddTagFromFilename, sourceSupportedExtensions);
        var addDestinationFolderCommand = new SetDestinationFolderCommand(collectionId, destFolderPath, destShouldCreateSubfoldersByHash, destShouldRenameByHash, "!format-error", "!hash-error", "!no-hash-error");
        
        await _httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        await _httpClient.PostAsJsonAsync("/collections/destination-folder", addDestinationFolderCommand);
        return new (collectionId, sourceFolderPath, destFolderPath);
    }

    private async Task<Guid> CreateCollection()
    {
        var collectionName = $"Collection Name {Guid.NewGuid()}";
        var result = await _httpClient.PostAsJsonAsync("/collections", new {name = collectionName});
        var resultString = await result.Content.ReadAsStringAsync();
        return Guid.Parse(resultString[1..^1]);
    }

    private record CreatedCollection(Guid Id, string SourceFolderPath, string DestFolderPath);
}
