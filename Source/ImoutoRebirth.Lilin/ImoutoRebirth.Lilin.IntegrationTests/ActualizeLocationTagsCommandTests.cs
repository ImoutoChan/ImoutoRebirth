using ImoutoRebirth.Lilin.Application.FileInfoSlice.Commands;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.IntegrationTests;

[Collection("WebApplication")]
public class ActualizeLocationTagsCommandTests(TestWebApplicationFactory<Program> _webApp)
{
    [Fact]
    public async Task ActualizeLocationTagsCommand()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var context = _webApp.GetDbContext(scope);

        string[] newTags = ["subfolder1", "subfolder2", "file-name1.jpg"];

        var command = new ActualizeLocationTagsCommand(
            Guid.NewGuid(),
            newTags);

        // act
        await mediator.Send(command);

        // assert
        var tags = context.Tags.Include(tagEntity => tagEntity.Type!);

        var firstTag = tags.FirstOrDefault(x => x.Name == newTags[0]);
        firstTag.Should().NotBeNull();
        firstTag.Type!.Name.Should().Be("Location");

        var secondTag = tags.FirstOrDefault(x => x.Name == newTags[1]);
        secondTag.Should().NotBeNull();
        secondTag.Type!.Name.Should().Be("Location");

        var thirdTag = tags.FirstOrDefault(x => x.Name == newTags[2]);
        thirdTag.Should().NotBeNull();
        thirdTag.Type!.Name.Should().Be("Location");

        var fileTags = context.FileTags.Include(x => x.Tag).ThenInclude(x => x!.Type!);

        var firstFileTag = fileTags.FirstOrDefault(x => x.Tag!.Name == newTags[0] && x.FileId == command.FileId);
        firstFileTag.Should().NotBeNull();

        var secondFileTag = fileTags.FirstOrDefault(x => x.Tag!.Name == newTags[1] && x.FileId == command.FileId);
        secondFileTag.Should().NotBeNull();

        var thirdFileTag = fileTags.FirstOrDefault(x => x.Tag!.Name == newTags[2] && x.FileId == command.FileId);
        thirdFileTag.Should().NotBeNull();
    }

    [Fact]
    public async Task ActualizeLocationTagsCommand_ShouldDeleteObsoleteLocationTags()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var context = _webApp.GetDbContext(scope);

        string[] obsoleteTags = ["subfolder1obsolete", "subfolder2obsolete", "obsolete-file-name1.jpg"];
        string[] newTags = ["subfolder1", "subfolder2", "file-name1.jpg"];
        var fileId = Guid.NewGuid();

        var obsoleteCommand = new ActualizeLocationTagsCommand(fileId, obsoleteTags);
        await mediator.Send(obsoleteCommand);

        var newCommand = new ActualizeLocationTagsCommand(fileId, newTags);

        // act
        await mediator.Send(newCommand);

        // assert
        var tags = context.Tags.Include(tagEntity => tagEntity.Type!).ToList();

        foreach (var newTag in newTags.Union(obsoleteTags))
        {
            var newTagEntity = tags.FirstOrDefault(x => x.Name == newTag);
            newTagEntity.Should().NotBeNull();
            newTagEntity.Type!.Name.Should().Be("Location");
        }

        var fileTags = context.FileTags.Include(x => x.Tag).ThenInclude(x => x!.Type!).ToList();

        foreach (var obsoleteTag in obsoleteTags)
        {
            var found = fileTags.FirstOrDefault(x => x.Tag!.Name == obsoleteTag && x.FileId == obsoleteCommand.FileId);
            found.Should().BeNull();
        }

        foreach (var newTag in newTags)
        {
            fileTags
                .FirstOrDefault(x => x.Tag!.Name == newTag && x.FileId == obsoleteCommand.FileId)
                .Should().NotBeNull();
        }
    }
}
