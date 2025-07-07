using AwesomeAssertions;
using ImoutoRebirth.Lamia.IntegrationTests.Fixtures;
using ImoutoRebirth.Lamia.MessageContracts;
using ImoutoRebirth.Lilin.MessageContracts;
using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ImoutoRebirth.Lamia.IntegrationTests;

[Collection("ApplicationFactory")]
public class ExtractFileMetadataCommandTests(LamiaApplicationFactory<Program> _app)
{
    [Fact]
    public async Task ExtractFileMetadataCommandFromVideoTest()
    {
        // arrange
        using var scope = _app.GetScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var file = new FileInfo(Path.Combine(_app.TestsLocation, "Resources", "video.mp4"));
        var fileId = Guid.NewGuid();

        // act
        await harness.Bus.Send(new ExtractFileMetadataCommand(file.FullName, fileId));

        // assert
        var sent = harness.Sent
            .Select<UpdateMetadataCommand>()
            .FirstOrDefault(x => x.Context.Message.FileId == fileId);

        sent.Should().NotBeNull();

        var message = sent.Context.Message;

        message.FileId.Should().Be(fileId);
        message.MetadataSource.Should().Be(MetadataSource.Lamia);
        message.FileNotes.Should().BeEmpty();
        message.FileTags.Should().NotBeEmpty();

        var tagsWithValues = message.FileTags.Where(x => x.Value != null).ToDictionary(x => x.Name, x => x.Value);
        var tagsWithoutValues = message.FileTags.Where(x => x.Value == null).Select(x => x.Name).ToList();

        tagsWithValues.Should().ContainKey("size");
        tagsWithValues["size"].Should().Be("92472");

        tagsWithValues.Should().ContainKey("width");
        tagsWithValues["width"].Should().Be("720");

        tagsWithValues.Should().ContainKey("height");
        tagsWithValues["height"].Should().Be("808");

        tagsWithValues.Should().ContainKey("duration");
        tagsWithValues["duration"].Should().Be("0:04");

        tagsWithoutValues.Should().Contain("type:video");
        tagsWithoutValues.Should().Contain("animated");
        tagsWithoutValues.Should().Contain("video");
        tagsWithoutValues.Should().Contain("mp4");
        tagsWithoutValues.Should().Contain("720x808");
        tagsWithoutValues.Should().Contain("aspect ratio:1.12");
        tagsWithoutValues.Should().Contain("vertical");
        tagsWithoutValues.Should().Contain("duration 5s");
        tagsWithoutValues.Should().Contain("h264");
        tagsWithoutValues.Should().Contain("30fps");
        tagsWithoutValues.Should().Contain("30+fps");

        tagsWithValues.Should().HaveCount(4);
        tagsWithoutValues.Should().HaveCount(11);
        message.FileTags.Should().HaveCount(15);

        message.FileTags.Should().AllSatisfy(tag => tag.Type.Should().Be("Meta"));
        message.FileTags.Should().AllSatisfy(tag => tag.Synonyms.Should().BeNull());
    }

    [Fact]
    public async Task ExtractFileMetadataCommandFromImageTest()
    {
        // arrange
        using var scope = _app.GetScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var file = new FileInfo(Path.Combine(_app.TestsLocation, "Resources", "image_jpg.jpg"));
        var fileId = Guid.NewGuid();

        // act
        await harness.Bus.Send(new ExtractFileMetadataCommand(file.FullName, fileId));

        // assert
        var sent = harness.Sent
            .Select<UpdateMetadataCommand>()
            .FirstOrDefault(x => x.Context.Message.FileId == fileId);

        sent.Should().NotBeNull();

        var message = sent.Context.Message;

        message.FileId.Should().Be(fileId);
        message.MetadataSource.Should().Be(MetadataSource.Lamia);
        message.FileNotes.Should().BeEmpty();
        message.FileTags.Should().NotBeEmpty();

        var tagsWithValues = message.FileTags.Where(x => x.Value != null).ToDictionary(x => x.Name, x => x.Value);
        var tagsWithoutValues = message.FileTags.Where(x => x.Value == null).Select(x => x.Name).ToList();

        tagsWithValues.Should().ContainKey("size");
        tagsWithValues["size"].Should().Be("348814");

        tagsWithValues.Should().ContainKey("width");
        tagsWithValues["width"].Should().Be("1920");

        tagsWithValues.Should().ContainKey("height");
        tagsWithValues["height"].Should().Be("2183");

        tagsWithoutValues.Should().Contain("type:image");
        tagsWithoutValues.Should().Contain("jpg");
        tagsWithoutValues.Should().Contain("1920x2183");
        tagsWithoutValues.Should().Contain("vertical");
        tagsWithoutValues.Should().Contain("aspect ratio:1.14");
        tagsWithoutValues.Should().Contain("mjpeg");

        tagsWithoutValues.Should().NotContain("sound");

        tagsWithValues.Should().HaveCount(3);
        tagsWithoutValues.Should().HaveCount(6);
        message.FileTags.Should().HaveCount(9);

        message.FileTags.Should().AllSatisfy(tag => tag.Type.Should().Be("Meta"));
        message.FileTags.Should().AllSatisfy(tag => tag.Synonyms.Should().BeNull());
    }

    [Fact]
    public async Task ExtractFileMetadataCommandFromAnimationTest()
    {
        // arrange
        using var scope = _app.GetScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var file = new FileInfo(Path.Combine(_app.TestsLocation, "Resources", "animated_png.png"));
        var fileId = Guid.NewGuid();

        // act
        await harness.Bus.Send(new ExtractFileMetadataCommand(file.FullName, fileId));

        // assert
        var sent = harness.Sent
            .Select<UpdateMetadataCommand>()
            .FirstOrDefault(x => x.Context.Message.FileId == fileId);

        sent.Should().NotBeNull();

        var message = sent.Context.Message;

        message.FileId.Should().Be(fileId);
        message.MetadataSource.Should().Be(MetadataSource.Lamia);
        message.FileNotes.Should().BeEmpty();
        message.FileTags.Should().NotBeEmpty();

        var tagsWithValues = message.FileTags.Where(x => x.Value != null).ToDictionary(x => x.Name, x => x.Value);
        var tagsWithoutValues = message.FileTags.Where(x => x.Value == null).Select(x => x.Name).ToList();

        tagsWithValues.Should().ContainKey("size");
        tagsWithValues["size"].Should().Be("951202");

        tagsWithValues.Should().ContainKey("width");
        tagsWithValues["width"].Should().Be("512");

        tagsWithValues.Should().ContainKey("height");
        tagsWithValues["height"].Should().Be("512");

        tagsWithValues.Should().ContainKey("animated frames");
        tagsWithValues["animated frames"].Should().Be("49");

        tagsWithoutValues.Should().Contain("type:animation");
        tagsWithoutValues.Should().Contain("animated");
        tagsWithoutValues.Should().Contain("png");
        tagsWithoutValues.Should().Contain("apng");
        tagsWithoutValues.Should().Contain("512x512");
        tagsWithoutValues.Should().Contain("square");
        tagsWithoutValues.Should().Contain("aspect ratio:1x1");

        tagsWithValues.Should().HaveCount(4);
        tagsWithoutValues.Should().HaveCount(7);
        message.FileTags.Should().HaveCount(11);

        message.FileTags.Should().AllSatisfy(tag => tag.Type.Should().Be("Meta"));
        message.FileTags.Should().AllSatisfy(tag => tag.Synonyms.Should().BeNull());
    }

    [Fact]
    public async Task ExtractFileMetadataCommandFromUgoiraTest()
    {
        // arrange
        using var scope = _app.GetScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var file = new FileInfo(Path.Combine(_app.TestsLocation, "Resources", "ugoira.zip"));
        var fileId = Guid.NewGuid();

        // act
        await harness.Bus.Send(new ExtractFileMetadataCommand(file.FullName, fileId));

        // assert
        var sent = harness.Sent
            .Select<UpdateMetadataCommand>()
            .FirstOrDefault(x => x.Context.Message.FileId == fileId);

        sent.Should().NotBeNull();

        var message = sent.Context.Message;

        message.FileId.Should().Be(fileId);
        message.MetadataSource.Should().Be(MetadataSource.Lamia);
        message.FileNotes.Should().BeEmpty();
        message.FileTags.Should().NotBeEmpty();

        var tagsWithValues = message.FileTags.Where(x => x.Value != null).ToDictionary(x => x.Name, x => x.Value);
        var tagsWithoutValues = message.FileTags.Where(x => x.Value == null).Select(x => x.Name).ToList();

        tagsWithValues.Should().ContainKey("size");
        tagsWithValues["size"].Should().Be("387163");

        tagsWithValues.Should().ContainKey("width");
        tagsWithValues["width"].Should().Be("730");

        tagsWithValues.Should().ContainKey("height");
        tagsWithValues["height"].Should().Be("1080");

        tagsWithValues.Should().ContainKey("animated frames");
        tagsWithValues["animated frames"].Should().Be("2");

        tagsWithValues.Should().ContainKey("archived files count");
        tagsWithValues["archived files count"].Should().Be("2");

        tagsWithoutValues.Should().Contain("type:animation");
        tagsWithoutValues.Should().Contain("730x1080");
        tagsWithoutValues.Should().Contain("vertical");
        tagsWithoutValues.Should().Contain("archived");
        tagsWithoutValues.Should().Contain("animated");
        tagsWithoutValues.Should().Contain("zip");
        tagsWithoutValues.Should().Contain("maybe ugoira");
        tagsWithoutValues.Should().Contain("aspect ratio:1.48");

        tagsWithValues.Should().HaveCount(5);
        tagsWithoutValues.Should().HaveCount(8);
        message.FileTags.Should().HaveCount(13);

        message.FileTags.Should().AllSatisfy(tag => tag.Type.Should().Be("Meta"));
        message.FileTags.Should().AllSatisfy(tag => tag.Synonyms.Should().BeNull());
    }

    [Fact]
    public async Task ExtractFileMetadataCommandFromComicTest()
    {
        // arrange
        using var scope = _app.GetScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var file = new FileInfo(Path.Combine(_app.TestsLocation, "Resources", "comic.cbz"));
        var fileId = Guid.NewGuid();

        // act
        await harness.Bus.Send(new ExtractFileMetadataCommand(file.FullName, fileId));

        // assert
        var sent = harness.Sent
            .Select<UpdateMetadataCommand>()
            .FirstOrDefault(x => x.Context.Message.FileId == fileId);

        sent.Should().NotBeNull();

        var message = sent.Context.Message;

        message.FileId.Should().Be(fileId);
        message.MetadataSource.Should().Be(MetadataSource.Lamia);
        message.FileNotes.Should().BeEmpty();
        message.FileTags.Should().NotBeEmpty();

        var tagsWithValues = message.FileTags.Where(x => x.Value != null).ToDictionary(x => x.Name, x => x.Value);
        var tagsWithoutValues = message.FileTags.Where(x => x.Value == null).Select(x => x.Name).ToList();

        tagsWithValues.Should().ContainKey("size");
        tagsWithValues["size"].Should().Be("434390");

        tagsWithValues.Should().ContainKey("width");
        tagsWithValues["width"].Should().Be("1280");

        tagsWithValues.Should().ContainKey("height");
        tagsWithValues["height"].Should().Be("894");

        tagsWithValues.Should().ContainKey("archived files count");
        tagsWithValues["archived files count"].Should().Be("3");

        tagsWithoutValues.Should().Contain("type:archive");
        tagsWithoutValues.Should().Contain("1280x894");
        tagsWithoutValues.Should().Contain("horizontal");
        tagsWithoutValues.Should().Contain("archived");
        tagsWithoutValues.Should().Contain("cbz");
        tagsWithoutValues.Should().Contain("aspect ratio:1.43");

        tagsWithValues.Should().HaveCount(4);
        tagsWithoutValues.Should().HaveCount(6);
        message.FileTags.Should().HaveCount(10);

        message.FileTags.Should().AllSatisfy(tag => tag.Type.Should().Be("Meta"));
        message.FileTags.Should().AllSatisfy(tag => tag.Synonyms.Should().BeNull());
    }

    [Fact]
    public async Task ExtractFileMetadataCommandFromGifTest()
    {
        // arrange
        using var scope = _app.GetScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var file = new FileInfo(Path.Combine(_app.TestsLocation, "Resources", "animated_gif.gif"));
        var fileId = Guid.NewGuid();

        // act
        await harness.Bus.Send(new ExtractFileMetadataCommand(file.FullName, fileId));

        // assert
        var sent = harness.Sent
            .Select<UpdateMetadataCommand>()
            .FirstOrDefault(x => x.Context.Message.FileId == fileId);

        sent.Should().NotBeNull();

        var message = sent.Context.Message;

        message.FileId.Should().Be(fileId);
        message.MetadataSource.Should().Be(MetadataSource.Lamia);
        message.FileNotes.Should().BeEmpty();
        message.FileTags.Should().NotBeEmpty();

        var tagsWithValues = message.FileTags.Where(x => x.Value != null).ToDictionary(x => x.Name, x => x.Value);
        var tagsWithoutValues = message.FileTags.Where(x => x.Value == null).Select(x => x.Name).ToList();

        tagsWithValues.Should().ContainKey("size");
        tagsWithValues["size"].Should().Be("274489");

        tagsWithValues.Should().ContainKey("width");
        tagsWithValues["width"].Should().Be("929");

        tagsWithValues.Should().ContainKey("height");
        tagsWithValues["height"].Should().Be("825");

        tagsWithValues.Should().ContainKey("animated frames");
        tagsWithValues["animated frames"].Should().Be("8");

        tagsWithoutValues.Should().Contain("type:animation");
        tagsWithoutValues.Should().Contain("929x825");
        tagsWithoutValues.Should().Contain("horizontal");
        tagsWithoutValues.Should().Contain("animated");
        tagsWithoutValues.Should().Contain("gif");
        tagsWithoutValues.Should().Contain("aspect ratio:1.13");

        tagsWithValues.Should().HaveCount(4);
        tagsWithoutValues.Should().HaveCount(6);
        message.FileTags.Should().HaveCount(10);

        message.FileTags.Should().AllSatisfy(tag => tag.Type.Should().Be("Meta"));
        message.FileTags.Should().AllSatisfy(tag => tag.Synonyms.Should().BeNull());
    }

    [Fact]
    public async Task ExtractFileMetadataCommandFromSoundVideoTest()
    {
        // arrange
        using var scope = _app.GetScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var file = new FileInfo(Path.Combine(_app.TestsLocation, "Resources", "video_sound.mp4"));
        var fileId = Guid.NewGuid();

        // act
        await harness.Bus.Send(new ExtractFileMetadataCommand(file.FullName, fileId));

        // assert
        var sent = harness.Sent
            .Select<UpdateMetadataCommand>()
            .FirstOrDefault(x => x.Context.Message.FileId == fileId);

        sent.Should().NotBeNull();

        var message = sent.Context.Message;

        message.FileId.Should().Be(fileId);
        message.MetadataSource.Should().Be(MetadataSource.Lamia);
        message.FileNotes.Should().BeEmpty();
        message.FileTags.Should().NotBeEmpty();

        var tagsWithValues = message.FileTags.Where(x => x.Value != null).ToDictionary(x => x.Name, x => x.Value);
        var tagsWithoutValues = message.FileTags.Where(x => x.Value == null).Select(x => x.Name).ToList();

        tagsWithValues.Should().ContainKey("size");
        tagsWithValues["size"].Should().Be("1182764");

        tagsWithValues.Should().ContainKey("width");
        tagsWithValues["width"].Should().Be("720");

        tagsWithValues.Should().ContainKey("height");
        tagsWithValues["height"].Should().Be("1280");

        tagsWithValues.Should().ContainKey("duration");
        tagsWithValues["duration"].Should().Be("0:10");

        tagsWithoutValues.Should().Contain("type:video");
        tagsWithoutValues.Should().Contain("animated");
        tagsWithoutValues.Should().Contain("video");
        tagsWithoutValues.Should().Contain("mp4");
        tagsWithoutValues.Should().Contain("720x1280");
        tagsWithoutValues.Should().Contain("vertical");
        tagsWithoutValues.Should().Contain("aspect ratio:16x9");
        tagsWithoutValues.Should().Contain("duration 10s");
        tagsWithoutValues.Should().Contain("sound");
        tagsWithoutValues.Should().Contain("h264");
        tagsWithoutValues.Should().Contain("audio aac");
        tagsWithoutValues.Should().Contain("30fps");
        tagsWithoutValues.Should().Contain("30+fps");

        tagsWithValues.Should().HaveCount(4);
        tagsWithoutValues.Should().HaveCount(13);
        message.FileTags.Should().HaveCount(17);

        message.FileTags.Should().AllSatisfy(tag => tag.Type.Should().Be("Meta"));
        message.FileTags.Should().AllSatisfy(tag => tag.Synonyms.Should().BeNull());
    }
}
