using AwesomeAssertions;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.Tests;
using ImoutoRebirth.Lamia.MessageContracts;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Room.Application.Cqrs;
using ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;
using ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;
using ImoutoRebirth.Room.Application.Cqrs.IntegritySlice;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Domain;
using ImoutoRebirth.Room.Domain.IntegrityAggregate;
using ImoutoRebirth.Room.IntegrationTests.Fixtures;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using System.Net.Http.Json;
using System.Text.Json;
using ImoutoRebirth.Meido.MessageContracts;
using Xunit;
using UpdateLocationTagsCommand = ImoutoRebirth.Lilin.MessageContracts.UpdateLocationTagsCommand;

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
            sourceSupportedExtensions:            ["jpg"],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        await _mediator.Send(new OverseeCommand());
        
        // act
        var response = await _httpClient.PostAsJsonAsync(
            "/collection-files",
            new CollectionFilesQuery(
                collectionId,
                [],
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
            sourceSupportedExtensions:            ["jpg"],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        await _mediator.Send(new OverseeCommand());
        
        // act
        var response = await _httpClient.PostAsJsonAsync(
            "/collection-files/filter-hashes",
            new FilterCollectionFileHashesQuery(
            [
                "5f30f9953332c230d11e3f26db5ae9a0",
                "09e56a8fd9d1e8beb62c50e6945632bf",
                "12356a8fd9d1e8beb62c50e6945632bf"
            ]));

        var files = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<string>>();
        
        // assert
        files.Should().HaveCount(2);
        files.Should().ContainAll(["5f30f9953332c230d11e3f26db5ae9a0", "09e56a8fd9d1e8beb62c50e6945632bf"]);
    }

    [Fact]
    public async Task FilterCollectionFilesShouldResetCache()
    {
        // arrange
        var (_, sourceFolderPath, _) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            ["jpg"],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var response1 = await _httpClient.PostAsJsonAsync(
            "/collection-files/filter-hashes",
            new FilterCollectionFileHashesQuery(
            [
                "5f30f9953332c230d11e3f26db5ae9a0",
                "09e56a8fd9d1e8beb62c50e6945632bf",
                "12356a8fd9d1e8beb62c50e6945632bf"
            ]));
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        await _mediator.Send(new OverseeCommand());
        
        // act
        var response2 = await _httpClient.PostAsJsonAsync(
            "/collection-files/filter-hashes",
            new FilterCollectionFileHashesQuery(
            [
                "5f30f9953332c230d11e3f26db5ae9a0",
                "09e56a8fd9d1e8beb62c50e6945632bf",
                "12356a8fd9d1e8beb62c50e6945632bf"
            ]));

        var files = await response2.Content.ReadFromJsonAsync<IReadOnlyCollection<string>>();
        
        // assert
        files.Should().HaveCount(2);
        files.Should().ContainAll(["5f30f9953332c230d11e3f26db5ae9a0", "09e56a8fd9d1e8beb62c50e6945632bf"]);
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
            sourceSupportedExtensions:            ["jpg"],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        await _mediator.Send(new OverseeCommand());
        
        // act
        var response = await _httpClient.PostAsJsonAsync(
            "/collection-files/search-ids",
            new CollectionFilesQuery(
                collectionId,
                [],
                null,
                null,
                5,
                0));

        var files = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<Guid>>();
        
        // assert
        files.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCollectionFileMetadata()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            ["jpg"],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);

        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));

        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        await _mediator.Send(new OverseeCommand());

        var fileId = _context.CollectionFiles
            .Where(x => x.CollectionId == collectionId)
            .Select(x => x.Id)
            .First();

        // act
        var response = await _httpClient.GetAsync($"/collection-files/{fileId}");
        var file = await response.Content
            .ReadFromJsonAsync<CollectionFileMetadata>(
                new JsonSerializerOptions(JsonSerializerOptions.Web)
                    .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

        // assert
        file.Should().NotBeNull();
        file!.Id.Should().Be(fileId);
        file.StoredMd5.Should().Be("5f30f9953332c230d11e3f26db5ae9a0");
        (SystemClock.Instance.GetCurrentInstant() - file.AddedOn).TotalMinutes.Should().BeLessThan(1);
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
            sourceSupportedExtensions:            ["jpg"],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        await _mediator.Send(new OverseeCommand());
        
        // act
        var response = await _httpClient.PostAsJsonAsync(
            "/collection-files/count",
            new CollectionFilesQuery(
                collectionId,
                [],
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
            sourceSupportedExtensions:            ["jpg"],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);

        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        
        await _mediator.Send(new OverseeCommand());
        await Task.Delay(250);
        
        var files = await _context.CollectionFiles
            .Where(x => x.CollectionId == collectionId)
            .ToListAsync();
        
        var file1 = files.FirstOrDefault(f => f.Md5 == "5f30f9953332c230d11e3f26db5ae9a0");
        var file2 = files.FirstOrDefault(f => f.Md5 == "09e56a8fd9d1e8beb62c50e6945632bf");
        file1.Should().NotBeNull();
        file2.Should().NotBeNull();

        // act
        var response = await _httpClient.DeleteAsync($"/collection-files/{file1.Id}");
        response.EnsureSuccessStatusCode();
        
        // assert
        var remainingFiles = await _context.CollectionFiles
            .Where(x => x.CollectionId == collectionId)
            .CountAsync();
        remainingFiles.Should().Be(1);

        File.Exists(Path.Combine(destFolderPath, testFile1.Name)).Should().BeFalse();
        File.Exists(Path.Combine(destFolderPath, "!Deleted", testFile1.Name)).Should().BeTrue();
        File.Exists(Path.Combine(destFolderPath, testFile2.Name)).Should().BeTrue();
        File.Exists(Path.Combine(destFolderPath, "!Deleted", testFile2.Name)).Should().BeFalse();
    }

    [Fact]
    public async Task UpdateLocationTags()
    {
        // arrange
        var collectionId = await CreateCollection();
        var collectionPath = Guid.NewGuid().ToString();
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "source");
        
        Directory.CreateDirectory(sourceFolderPath);
        
        var addSourceFolderCommand = new AddSourceFolderCommand(collectionId, sourceFolderPath, false, false, true, true, ["jpg"], false, null);
        
        await _httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));

        Directory.CreateDirectory(Path.Combine(sourceFolderPath, "inner"));
        testFile1.CopyTo(Path.Combine(sourceFolderPath, "inner", testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, "inner", testFile2.Name));
        await _mediator.Send(new OverseeCommand());
        
        // act
        await _httpClient.PostAsync("/collection-files/updateSourceTags", null);

        var files = await _context.CollectionFiles.Where(x => x.CollectionId == collectionId).ToListAsync();
        
        // assert

        _harness.Sent.SelectMessages<UpdateLocationTagsCommand>().Any(x =>
            x.FileId == files[0].Id || x.FileId == files[1].Id
            && x.LocationTags.Any(y => y is "inner")
            && x.LocationTags.Any(y => y is "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg")
            && x.LocationTags.Length == 2
        ).Should().BeTrue();

        _harness.Sent.SelectMessages<UpdateLocationTagsCommand>().Any(x =>
            x.FileId == files[0].Id || x.FileId == files[1].Id
            && x.LocationTags.Any(y => y is "inner")
            && x.LocationTags.Any(y => y is "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg")
            && x.LocationTags.Length == 2
        ).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateFileMetadata()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            ["jpg"],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);

        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));

        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        await _mediator.Send(new OverseeCommand());

        var files = await _context.CollectionFiles.Where(x => x.CollectionId == collectionId).ToListAsync();

        // act
        await _mediator.Send(new UpdateFileMetadataCommand());

        // assert
        var sentMessages = _harness.Sent.SelectMessages<ExtractFileMetadataCommand>().ToList();

        var ourFileMessages = sentMessages
            .Where(x => files.Any(f => f.Id == x.FileId && f.Path == x.FileFullName))
            .ToList();

        // events are sent twice, first on the initial addition to the collection,
        // and then again when UpdateFileMetadataCommand is fired
        ourFileMessages.Should().HaveCount(files.Count * 2);
    }

    [Fact]
    public async Task UpdateFileMetadata_MultipleCollections()
    {
        // arrange
        var (collectionId1, sourceFolderPath1, destFolderPath1) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            ["jpg"],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);

        var (collectionId2, sourceFolderPath2, destFolderPath2) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            ["jpg"],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);

        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));

        testFile1.CopyTo(Path.Combine(sourceFolderPath1, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath2, testFile2.Name));
        await _mediator.Send(new OverseeCommand());

        var collection1Files = await _context.CollectionFiles.Where(x => x.CollectionId == collectionId1).ToListAsync();
        var collection2Files = await _context.CollectionFiles.Where(x => x.CollectionId == collectionId2).ToListAsync();
        var files = collection1Files.Union(collection2Files).ToList();

        // act
        await _mediator.Send(new UpdateFileMetadataCommand());

        // assert
        var sentMessages = _harness.Sent.SelectMessages<ExtractFileMetadataCommand>().ToList();

        var ourFileMessages = sentMessages
            .Where(x => files.Any(f => f.Id == x.FileId && f.Path == x.FileFullName))
            .ToList();

        // events are sent twice, first on the initial addition to the collection,
        // and then again when UpdateFileMetadataCommand is fired
        ourFileMessages.Should().HaveCount(files.Count * 2);
    }

    [Fact]
    public async Task Rename_WithSimpleCollection_ShouldRenameFileInDbAndOnDisk()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat: false,
            sourceShouldCheckHashFromName: false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename: false,
            sourceSupportedExtensions: ["jpg"],
            destShouldCreateSubfoldersByHash: false,
            destShouldRenameByHash: false);

        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));

        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        await _mediator.Send(new OverseeCommand());

        var files = await _context.CollectionFiles.AsNoTracking().Where(x => x.CollectionId == collectionId).ToListAsync();
        var firstFile = files.First();
        var newName = "new name.png";

        // act
        var response = await _webApp.Client.PatchAsync(
            $"/collection-files/{firstFile.Id}/rename?newFileName={newName}",
            null);

        // assert
        response.EnsureSuccessStatusCode();
        var renamedFileInDb = await _context.CollectionFiles.AsNoTracking().Where(x => x.Id == firstFile.Id).FirstAsync();
        renamedFileInDb.Path.Should().Be(Path.Combine(destFolderPath, newName));

        File.Exists(renamedFileInDb.Path).Should().BeTrue();

        _harness.Sent.SelectMessages<ProcessRenamedFileCommand>().Any(x
                => x.FileId == firstFile.Id && x.Md5 == firstFile.Md5 && x.NewFileName == newName)
            .Should().BeTrue();

        _harness.Sent.SelectMessages<ExtractFileMetadataCommand>().Any(x
                => x.FileId == firstFile.Id && x.FileFullName == renamedFileInDb.Path)
            .Should().BeTrue();
    }

    [Fact]
    public async Task Rename_WithSubFoldersCollection_ShouldRenameFileInDbAndOnDisk()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat: false,
            sourceShouldCheckHashFromName: false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename: false,
            sourceSupportedExtensions: ["jpg"],
            destShouldCreateSubfoldersByHash: true,
            destShouldRenameByHash: false);

        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));

        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        await _mediator.Send(new OverseeCommand());

        var files = await _context.CollectionFiles.AsNoTracking().Where(x => x.CollectionId == collectionId).ToListAsync();
        var firstFile = files.First();
        var newName = "new name.png";

        // act
        var response = await _webApp.Client.PatchAsync(
            $"/collection-files/{firstFile.Id}/rename?newFileName={newName}",
            null);

        // assert
        response.EnsureSuccessStatusCode();
        var renamedFileInDb = await _context.CollectionFiles.AsNoTracking().Where(x => x.Id == firstFile.Id).FirstAsync();
        renamedFileInDb.Path.Should().Be(Path.Combine(Path.GetDirectoryName(firstFile.Path)!, newName));

        File.Exists(renamedFileInDb.Path).Should().BeTrue();

        _harness.Sent.SelectMessages<ProcessRenamedFileCommand>().Any(x
                => x.FileId == firstFile.Id && x.Md5 == firstFile.Md5 && x.NewFileName == newName)
            .Should().BeTrue();

        _harness.Sent.SelectMessages<ExtractFileMetadataCommand>().Any(x
                => x.FileId == firstFile.Id && x.FileFullName == renamedFileInDb.Path)
            .Should().BeTrue();
    }

    [Fact]
    public async Task Rename_WithRenamingCollection_ShouldThrow()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat: false,
            sourceShouldCheckHashFromName: false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename: false,
            sourceSupportedExtensions: ["jpg"],
            destShouldCreateSubfoldersByHash: true,
            destShouldRenameByHash: true);

        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));

        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile2.Name));
        await _mediator.Send(new OverseeCommand());

        var files = await _context.CollectionFiles.AsNoTracking().Where(x => x.CollectionId == collectionId).ToListAsync();
        var firstFile = files.First();
        var newName = "new name.png";

        // act
        var action = async () =>
        {
            var response = await _webApp.Client.PatchAsync(
                $"/collection-files/{firstFile.Id}/rename?newFileName={newName}",
                null);
            response.EnsureSuccessStatusCode();
        };

        // assert
        await Assert.ThrowsAsync<HttpRequestException>(action);
    }

    [Fact]
    public async Task Rename_WithoutDestinationFolder_ShouldUpdateLocationTags()
    {
        // arrange
        var collectionId = await CreateCollection();
        var collectionPath = Guid.NewGuid().ToString();

        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "source");

        Directory.CreateDirectory(sourceFolderPath);

        var addSourceFolderCommand = new AddSourceFolderCommand(
            CollectionId: collectionId,
            Path: sourceFolderPath,
            ShouldCheckFormat: false,
            ShouldCheckHashFromName: false,
            ShouldCreateTagsFromSubfolders: true,
            ShouldAddTagFromFilename: true,
            SupportedExtensions: [],
            IsWebhookUploadEnabled: false,
            WebhookUploadUrl: null);

        await _httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);

        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));

        var subfolder1 = Path.Combine(sourceFolderPath, "subfolder1", "subfolder2");
        Directory.CreateDirectory(subfolder1);

        testFile1.CopyTo(Path.Combine(subfolder1, testFile1.Name));
        await _mediator.Send(new OverseeCommand());

        var files = await _context.CollectionFiles.AsNoTracking().Where(x => x.CollectionId == collectionId).ToListAsync();
        var firstFile = files.First();
        var newName = "new name.png";

        // act
        var response = await _webApp.Client.PatchAsync(
            $"/collection-files/{firstFile.Id}/rename?newFileName={newName}",
            null);

        // assert
        response.EnsureSuccessStatusCode();
        var renamedFileInDb = await _context.CollectionFiles.AsNoTracking().Where(x => x.Id == firstFile.Id).FirstAsync();
        renamedFileInDb.Path.Should().Be(Path.Combine(Path.GetDirectoryName(firstFile.Path)!, newName));

        File.Exists(renamedFileInDb.Path).Should().BeTrue();

        _harness.Sent.SelectMessages<UpdateLocationTagsCommand>().Any(x =>
            x.FileId == files[0].Id
            && x.LocationTags.Any(y => y is "subfolder1")
            && x.LocationTags.Any(y => y is "subfolder2")
            && x.LocationTags.Any(y => y is "new name.png")
            && x.LocationTags.Length == 3
        ).Should().BeTrue();

        _harness.Sent.SelectMessages<ProcessRenamedFileCommand>().Any(x
                => x.FileId == firstFile.Id && x.Md5 == firstFile.Md5 && x.NewFileName == newName)
            .Should().BeTrue();

        _harness.Sent.SelectMessages<ExtractFileMetadataCommand>().Any(x
                => x.FileId == firstFile.Id && x.FileFullName == renamedFileInDb.Path)
            .Should().BeTrue();
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
        sourceSupportedExtensions ??= [];
        
        var collectionId = await CreateCollection();
        var collectionPath = Guid.NewGuid().ToString();
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "source");
        var destFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "dest");
        
        Directory.CreateDirectory(sourceFolderPath);
        Directory.CreateDirectory(destFolderPath);

        var addSourceFolderCommand = new AddSourceFolderCommand(
            collectionId,
            sourceFolderPath,
            sourceShouldCheckFormat,
            sourceShouldCheckHashFromName,
            sourceShouldCreateTagsFromSubfolders,
            sourceShouldAddTagFromFilename,
            sourceSupportedExtensions,
            false,
            null);

        var addDestinationFolderCommand = new SetDestinationFolderCommand(
            collectionId,
            destFolderPath,
            destShouldCreateSubfoldersByHash,
            destShouldRenameByHash,
            "!format-error",
            "!hash-error",
            "!no-hash-error");
        
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
