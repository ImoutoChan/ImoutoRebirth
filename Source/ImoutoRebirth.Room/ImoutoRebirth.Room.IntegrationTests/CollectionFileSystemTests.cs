using System.Net.Http.Json;
using AwesomeAssertions;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lamia.MessageContracts;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Room.Application.Cqrs;
using ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.IntegrationTests.Fixtures;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
 * + Check flags: images with wrong hash in the name are moved to the bad hash folder
 * + Check flags: images without hash moved to the no hash folder
 * + Check flags: images push its name to the tags
 * + Check flags: images push its folder name to the tags
 * 
 * + Check flags [false]: bad images are processed correctly if format check is disabled
 * + Check flags [false]: images with wrong hash in the name are processed correctly if hash check is disabled
 * + Check flags [false]: images without hash are processed correctly if no hash check is disabled
 * + Check flags [false]: images don't push its name to the tags if the corresponding flag is disabled
 * + Check flags [false]: images don't push its folder name to the tags if the corresponding flag is disabled
 * 
 * + Formats: all files are processed if formats are empty
 * + Formats: only files with the specified extensions are processed if formats are specified
 * 
 * # Destination folder options
 * + Check flags: files are renamed if the corresponding flag is enabled
 * + Check flags: files are moved to the hash subfolder if the corresponding flag is enabled
 * + Check flags [false]: files are not renamed if the corresponding flag is disabled
 * + Check flags [false]: files are not moved to the hash subfolder if the corresponding flag is disabled
 *
 */

[Collection("WebApplication")]
public class CollectionFileSystemTests : IDisposable
{
    private readonly TestWebApplicationFactory<Program> _webApp;
    private readonly IServiceScope _scope;
    private readonly RoomDbContext _context;
    private readonly IMediator _mediator;
    private readonly ITestHarness _harness;
    private readonly HttpClient _httpClient;

    public CollectionFileSystemTests(TestWebApplicationFactory<Program> webApp)
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
    public async Task FileInSourceFolderShouldBeAddedToDatabaseAndMovedToDestinationFolder()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              true,
            sourceShouldCheckHashFromName:        true,
            sourceShouldCreateTagsFromSubfolders: true,
            sourceShouldAddTagFromFilename:       true,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);

        // act
        var file = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        file.CopyTo(Path.Combine(sourceFolderPath, file.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        var savedFile = await _context.CollectionFiles.FirstOrDefaultAsync(x => x.CollectionId == collectionId);

        savedFile.Should().NotBeNull();
        savedFile!.Path.Should().Be(Path.Combine(destFolderPath, "5f", "30", "5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        savedFile.Md5.Should().Be("5f30f9953332c230d11e3f26db5ae9a0");
        savedFile.Size.Should().Be(150881);
        savedFile.IsRemoved.Should().BeFalse();
        savedFile.OriginalPath.Should().Be(Path.Combine(sourceFolderPath, file.Name));

        _harness.Sent
            .AnyMessage<NewFileCommand>(
                x => x.FileId == savedFile.Id && x is
                {
                    Md5: "5f30f9953332c230d11e3f26db5ae9a0",
                    FileName: "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"
                })
            .Should().BeTrue();

        _harness.Sent
            .AnyMessage<ExtractFileMetadataCommand>(x
                => x.FileId == savedFile.Id
                   && x.FileFullName == savedFile.Path)
            .Should().BeTrue();

        _harness.Sent
            .AnyMessage<UpdateMetadataCommand>(x
                => x.FileId == savedFile.Id
                   && x.MetadataSource == MetadataSource.Manual
                   && x.FileTags.Any(y => y is { Type: "Location", Name: "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg" }))
            .Should().BeTrue();
    }

    [Fact]
    public async Task FileInSourceFolderWithoutDestinationShouldBeAddedToDatabaseAndNotMoved()
    {
        // arrange
        var collectionId = await CreateCollection();
        var collectionPath = Guid.NewGuid().ToString();
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collections", collectionPath, "source");
        var innerSourceFolderPath = Path.Combine(sourceFolderPath, "inner");
        Directory.CreateDirectory(innerSourceFolderPath);
        
        var addSourceFolderCommand = new AddSourceFolderCommand(collectionId, sourceFolderPath, true, true, true, true, new[] { "jpg" });
        await _httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        
        // act
        var file = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFilePath = Path.Combine(innerSourceFolderPath, file.Name);
        file.CopyTo(testFilePath);
        
        await _mediator.Send(new OverseeCommand());
        
        // assert
        var savedFile = await _context.CollectionFiles.FirstOrDefaultAsync(x => x.CollectionId == collectionId);

        savedFile.Should().NotBeNull();
        savedFile!.Path.Should().Be(testFilePath);
        savedFile.Md5.Should().Be("5f30f9953332c230d11e3f26db5ae9a0");
        savedFile.Size.Should().Be(150881);
        savedFile.IsRemoved.Should().BeFalse();
        savedFile.OriginalPath.Should().Be(testFilePath);

        _harness.Sent
            .AnyMessage<NewFileCommand>(
                x => x.FileId == savedFile.Id
                     && x.Md5 == "5f30f9953332c230d11e3f26db5ae9a0"
                     && x.FileName == file.Name)
            .Should().BeTrue();

        _harness.Sent
            .AnyMessage<ExtractFileMetadataCommand>(x
                => x.FileId == savedFile.Id
                   && x.FileFullName == savedFile.Path)
            .Should().BeTrue();

        _harness.Sent
            .AnyMessage<UpdateMetadataCommand>(x
                => x.FileId == savedFile.Id
                   && x.MetadataSource == MetadataSource.Manual
                   && x.FileTags.Any(y => y is { Type: "Location", Name: "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg" })
                   && x.FileTags.Any(y => y is { Type: "Location", Name: "inner" }))
            .Should().BeTrue();
    }

    [Fact]
    public async Task DuplicateImagesAreDeletedFromSourceFolder()
    {
        // arrange
        var (collectionId, sourceFolderPath, _) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              true,
            sourceShouldCheckHashFromName:        true,
            sourceShouldCreateTagsFromSubfolders: true,
            sourceShouldAddTagFromFilename:       true,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());

        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
    }

    [Fact]
    public async Task ImagesWithSameMd5AreDeletedFromSourceFolder()
    {
        // arrange
        var (collectionId, sourceFolderPath, _) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              true,
            sourceShouldCheckHashFromName:        true,
            sourceShouldCreateTagsFromSubfolders: true,
            sourceShouldAddTagFromFilename:       true,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());

        testFile.CopyTo(Path.Combine(sourceFolderPath, "diff-name" + testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
    }

    [Fact]
    public async Task ImagesWithSameNameButDifferentMd5AreProcessedCorrectly()
    {
        // arrange
        var (collectionId, sourceFolderPath, _) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              true,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: true,
            sourceShouldAddTagFromFilename:       true,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        
        // act
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        await _mediator.Send(new OverseeCommand());

        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(2);
    }

    [Fact]
    public async Task BrokenImagesAreMovedToFormatErrorFolder()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              true,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: true,
            sourceShouldAddTagFromFilename:       true,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "empty-file.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(0);
        Directory.GetFiles(Path.Combine(destFolderPath, "!format-error")).Should().NotBeEmpty();
    }

    [Fact]
    public async Task BrokenImagesAreNotMovedToFormatErrorFolderWhenFormatCheckIsDisabled()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: true,
            sourceShouldAddTagFromFilename:       true,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "empty-file.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
        Directory.Exists(Path.Combine(destFolderPath, "!format-error")).Should().BeFalse();
    }

    [Fact]
    public async Task WrongHashImagesAreMovedToHashErrorFolder()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        true,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        // rename to wrong hash
        testFile.CopyTo(Path.Combine(sourceFolderPath, "file1-1f30f9953332c230d11e3f26db5ae9a0.jpg"));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(0);
        Directory.GetFiles(Path.Combine(destFolderPath, "!hash-error")).Should().NotBeEmpty();
    }

    [Fact]
    public async Task WrongHashImagesAreNotMovedToHashErrorFolderWhenHashCheckIsDisabled()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        // rename to wrong hash
        testFile.CopyTo(Path.Combine(sourceFolderPath, "file1-1f30f9953332c230d11e3f26db5ae9a0.jpg"));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
        Directory.Exists(Path.Combine(destFolderPath, "!hash-error")).Should().BeFalse();
    }

    [Fact]
    public async Task NoHashImagesAreMovedToNoHashFolder()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        true ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "empty-file.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, "empty-file.jpg"));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(0);
        Directory.GetFiles(Path.Combine(destFolderPath, "!no-hash-error")).Should().NotBeEmpty();
    }

    [Fact]
    public async Task NoHashImagesAreNotMovedToNoHashFolderWhenHashCheckIsDisabled()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "empty-file.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, "empty-file.jpg"));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
        Directory.Exists(Path.Combine(destFolderPath, "!no-hash-error")).Should().BeFalse();
    }

    [Fact]
    public async Task ImageNameIsPushedAsTag()
    {
        // arrange
        var (collectionId, sourceFolderPath, _) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       true,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
        var dbFile = _context.CollectionFiles.First(x => x.CollectionId == collectionId);

        _harness.Sent
            .AnyMessage<UpdateMetadataCommand>(x
                => x.FileId == dbFile.Id
                   && x.MetadataSource == MetadataSource.Manual
                   && x.FileTags.Any(y => y is { Type: "Location", Name: "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg" }))
            .Should().BeTrue();
    }

    [Fact]
    public async Task ImageNameIsNotPushedAsTagWhenShouldAddTagFromFilenameDisabled()
    {
        // arrange
        var (collectionId, sourceFolderPath, _) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            new[] { "jpg" },
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
        var dbFile = _context.CollectionFiles.First(x => x.CollectionId == collectionId);
        
        _harness.Sent
            .AnyMessage<UpdateMetadataCommand>(x
                => x.FileId == dbFile.Id && x.FileTags.Any(y => y.Name == "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"))
            .Should().BeFalse();
    }

    [Fact]
    public async Task ImageSubfolderIsPushedAsTag()
    {
        // arrange
        var collectionId = await CreateCollection();
        var collectionPath = Guid.NewGuid().ToString();
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collections", collectionPath, "source");
        var innerSourceFolderPath = Path.Combine(sourceFolderPath, "inner");
        Directory.CreateDirectory(innerSourceFolderPath);
        
        var addSourceFolderCommand = new AddSourceFolderCommand(collectionId, sourceFolderPath, true, true, true, true, new[] { "jpg" });
        await _httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        
        // act
        var file = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFilePath = Path.Combine(innerSourceFolderPath, file.Name);
        file.CopyTo(testFilePath);
        
        await _mediator.Send(new OverseeCommand());
        
        // assert
        var savedFile = await _context.CollectionFiles.FirstAsync(x => x.CollectionId == collectionId);

        _harness.Sent
            .AnyMessage<NewFileCommand>(
                x => x.FileId == savedFile.Id
                     && x is
                     {
                         Md5: "5f30f9953332c230d11e3f26db5ae9a0",
                         FileName: "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"
                     })
            .Should().BeTrue();

        _harness.Sent
            .AnyMessage<ExtractFileMetadataCommand>(x
                => x.FileId == savedFile.Id
                   && x.FileFullName == savedFile.Path)
            .Should().BeTrue();

        _harness.Sent
            .AnyMessage<UpdateMetadataCommand>(x
                => x.FileId == savedFile.Id
                   && x.MetadataSource == MetadataSource.Manual
                   && x.FileTags.Any(y => y is { Type: "Location", Name: "inner" }))
            .Should().BeTrue();
    }

    [Fact]
    public async Task ImageSubfolderIsNotPushedAsTagWhenShouldCreateTagsFromSubfoldersIsDisabled()
    {
        // arrange
        var collectionId = await CreateCollection();
        var collectionPath = Guid.NewGuid().ToString();
        
        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collections", collectionPath, "source");
        var innerSourceFolderPath = Path.Combine(sourceFolderPath, "inner");
        Directory.CreateDirectory(innerSourceFolderPath);
        
        var addSourceFolderCommand = new AddSourceFolderCommand(collectionId, sourceFolderPath, true, true, false, true, new[] { "jpg" });
        await _httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        
        // act
        var file = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFilePath = Path.Combine(innerSourceFolderPath, file.Name);
        file.CopyTo(testFilePath);
        
        await _mediator.Send(new OverseeCommand());
        
        // assert
        var savedFile = await _context.CollectionFiles.FirstAsync(x => x.CollectionId == collectionId);

        _harness.Sent
            .AnyMessage<NewFileCommand>(
                x => x.FileId == savedFile.Id
                     && x.Md5 == "5f30f9953332c230d11e3f26db5ae9a0"
                     && x.FileName == file.Name)
            .Should().BeTrue();

        _harness.Sent
            .AnyMessage<ExtractFileMetadataCommand>(x
                => x.FileId == savedFile.Id
                   && x.FileFullName == savedFile.Path)
            .Should().BeTrue();

        _harness.Sent
            .AnyMessage<UpdateMetadataCommand>(x
                => x.FileId == savedFile.Id
                   && x.MetadataSource == MetadataSource.Manual
                   && x.FileTags.Any(y => y is { Type: "Location", Name: "inner" }))
            .Should().BeFalse();
    }

    [Fact]
    public async Task ImagesAreIgnoredWhenArentMatchedByExtensions()
    {
        // arrange
        var (collectionId, sourceFolderPath, _) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            ["png"],
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().NotBeEmpty();
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(0);
    }

    [Fact]
    public async Task ImagesAreNotIgnoredWhenExtensionsFilterIsEmpty()
    {
        // arrange
        var (collectionId, sourceFolderPath, _) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            [],
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
    }

    [Fact]
    public async Task ImagesAreNotIgnoredWhenExtensionsFilterIsMatched()
    {
        // arrange
        var (collectionId, sourceFolderPath, _) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            new []{ "jpg" },
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
    }

    [Fact]
    public async Task FileIsRenamedToHash()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            [],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        Directory.GetFiles(destFolderPath).Should().HaveCount(1);
        Directory.GetFiles(destFolderPath).First().Should().Be(Path.Combine(destFolderPath, "5f30f9953332c230d11e3f26db5ae9a0.jpg"));
    }

    [Fact]
    public async Task FileIsNotRenamedToHashWhenShouldRenameByHashIsDisabled()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            [],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        Directory.GetFiles(destFolderPath).Should().HaveCount(1);
        Directory.GetFiles(destFolderPath).First().Should().Be(Path.Combine(destFolderPath, "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
    }

    [Fact]
    public async Task FileIsMovedToHashFolder()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            [],
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        Directory.GetFiles(destFolderPath, "*", SearchOption.AllDirectories).Should().HaveCount(1);
        Directory.GetFiles(destFolderPath, "*", SearchOption.AllDirectories).First()
            .Should().Be(Path.Combine(destFolderPath, "5f", "30", "5f30f9953332c230d11e3f26db5ae9a0.jpg"));
    }

    [Fact]
    public async Task FileIsNotMovedToHashFolder()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            [],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               true);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(1);
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        Directory.GetFiles(destFolderPath).Should().HaveCount(1);
        Directory.GetFiles(destFolderPath).First().Should().Be(Path.Combine(destFolderPath, "5f30f9953332c230d11e3f26db5ae9a0.jpg"));
    }

    [Fact]
    public async Task FileIsMovedToHashFolderIsNotRenamedWithSameName()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            [],
            destShouldCreateSubfoldersByHash:     true,
            destShouldRenameByHash:               false);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        
        // act
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        await _mediator.Send(new OverseeCommand());
            
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(2);
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        Directory.GetFiles(destFolderPath, "*", SearchOption.AllDirectories).Should().HaveCount(2);
        Directory.GetFiles(destFolderPath, "*", SearchOption.AllDirectories).First()
            .Should().Be(Path.Combine(destFolderPath, "09", "e5", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        Directory.GetFiles(destFolderPath, "*", SearchOption.AllDirectories).ElementAt(1)
            .Should().Be(Path.Combine(destFolderPath, "5f", "30", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
    }

    [Fact]
    public async Task FileIsMovedToDestinationWithSameName()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            [],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        
        // act
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        await _mediator.Send(new OverseeCommand());
            
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(2);
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        Directory.GetFiles(destFolderPath, "*", SearchOption.AllDirectories).Should().HaveCount(2);
        Directory.GetFiles(destFolderPath, "*", SearchOption.AllDirectories).First()
            .Should().Be(Path.Combine(destFolderPath, "file1-5f30f9953332c230d11e3f26db5ae9a0 (0).jpg"));
        Directory.GetFiles(destFolderPath, "*", SearchOption.AllDirectories).ElementAt(1)
            .Should().Be(Path.Combine(destFolderPath, "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
    }

    [Fact]
    public async Task MultipleFileIsMovedToDestinationWithSameName()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            [],
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var testFile1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var testFile2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));
        var testFile3 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "empty-file.jpg"));
        
        // act
        testFile1.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        await _mediator.Send(new OverseeCommand());
            
        testFile2.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        await _mediator.Send(new OverseeCommand());
            
        testFile3.CopyTo(Path.Combine(sourceFolderPath, testFile1.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        _context.CollectionFiles.Count(x => x.CollectionId == collectionId).Should().Be(3);
        Directory.GetFiles(sourceFolderPath).Should().BeEmpty();
        Directory.GetFiles(destFolderPath, "*", SearchOption.AllDirectories).Should().HaveCount(3);
        Directory.GetFiles(destFolderPath, "*", SearchOption.AllDirectories).First()
            .Should().Be(Path.Combine(destFolderPath, "file1-5f30f9953332c230d11e3f26db5ae9a0 (0).jpg"));
        Directory.GetFiles(destFolderPath, "*", SearchOption.AllDirectories).ElementAt(1)
            .Should().Be(Path.Combine(destFolderPath, "file1-5f30f9953332c230d11e3f26db5ae9a0 (1).jpg"));
        Directory.GetFiles(destFolderPath, "*", SearchOption.AllDirectories).ElementAt(2)
            .Should().Be(Path.Combine(destFolderPath, "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
    }

    [Fact]
    public async Task ImoutoPicsUploadCalledWhenImoutoPicsUploaderEnabled()
    {
        // arrange
        var (_, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            new []{ "jpg" },
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        await _webApp.Client.PostAsync("/imouto-pics-uploader-enabled", null);
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        var file = Path.Combine(destFolderPath, "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg");
        _webApp.ImoutoPicsUploaderMock.Verify(x => x.UploadFile(file), Times.Once);
    }

    [Fact()]
    public async Task ImoutoPicsUploadShouldNotBeCalledWhenImoutoPicsUploaderDisabled()
    {
        // arrange
        var (_, sourceFolderPath, destFolderPath) = await CreateDefaultCollection(
            sourceShouldCheckFormat:              false,
            sourceShouldCheckHashFromName:        false ,
            sourceShouldCreateTagsFromSubfolders: false,
            sourceShouldAddTagFromFilename:       false,
            sourceSupportedExtensions:            new []{ "jpg" },
            destShouldCreateSubfoldersByHash:     false,
            destShouldRenameByHash:               false);
        
        var testFile = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        
        await _webApp.Client.DeleteAsync("/imouto-pics-uploader-enabled");
        
        // act
        testFile.CopyTo(Path.Combine(sourceFolderPath, testFile.Name));
        await _mediator.Send(new OverseeCommand());
        
        // assert
        var file = Path.Combine(destFolderPath, "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg");
        _webApp.ImoutoPicsUploaderMock.Verify(x => x.UploadFile(file), Times.Never);
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
