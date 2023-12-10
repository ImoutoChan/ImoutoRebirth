using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Commands;
using ImoutoRebirth.Meido.MessageContracts;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.IntegrationTests;

[Collection("WebApplication")]
public class ActualizeFileInfoForSourceCommandTests(TestWebApplicationFactory<Program> _webApp)
{
    [Fact]
    public async Task ActualizeFileInfoForSourceCommand()
    {
        // arrange
        var scope = _webApp.GetScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var context = _webApp.GetDbContext(scope);

        ActualizeTag[] newTags =
        [
            new("General", "1girl" + Guid.NewGuid(), null, null, Domain.TagAggregate.TagOptions.None),
            new("General", "solo" + Guid.NewGuid(), null, null, Domain.TagAggregate.TagOptions.None),
            new("General", "huge melons" + Guid.NewGuid(), null, null, Domain.TagAggregate.TagOptions.None),
            new("General", "Rating" + Guid.NewGuid(), "Questionable", new[] { "Rate" }, Domain.TagAggregate.TagOptions.None),
        ];
        
        var command = new ActualizeFileInfoForSourceCommand(
            Guid.NewGuid(),
            ImoutoRebirth.Lilin.Domain.FileInfoAggregate.MetadataSource.Danbooru,
            newTags,
            [
                new(
                    SourceId: 1,
                    Label: "Bounce sound translation" + Guid.NewGuid(),
                    PositionFromLeft: 10,
                    PositionFromTop: 20,
                    Width: 30,
                    Height: 40)
            ]);
        
        // act
        await mediator.Send(command);

        // assert
        var tags = context.Tags.Include(tagEntity => tagEntity.Type!);
        
        var firstTag = tags.FirstOrDefault(x => x.Name == newTags[0].Name);
        firstTag.Should().NotBeNull();
        firstTag!.Type!.Name.Should().Be(newTags[0].Type);
        
        var secondTag = tags.FirstOrDefault(x => x.Name == newTags[1].Name);
        secondTag.Should().NotBeNull();
        secondTag!.Type!.Name.Should().Be(newTags[1].Type);
        
        var thirdTag = tags.FirstOrDefault(x => x.Name == newTags[2].Name);
        thirdTag.Should().NotBeNull();
        thirdTag!.Type!.Name.Should().Be(newTags[2].Type);
        
        var fourthTag = tags.FirstOrDefault(x => x.Name == newTags[3].Name);
        fourthTag.Should().NotBeNull();
        fourthTag!.Type!.Name.Should().Be(newTags[3].Type);
        fourthTag.HasValue.Should().BeTrue();
        fourthTag.SynonymsArray.Should().BeEquivalentTo(newTags[3].Synonyms);

        var fileTags = context.FileTags.Include(x => x.Tag).ThenInclude(x => x!.Type!);

        var firstFileTag = fileTags.FirstOrDefault(x => x.Tag!.Name == newTags[0].Name && x.FileId == command.FileId);
        firstFileTag.Should().NotBeNull();

        var secondFileTag = fileTags.FirstOrDefault(x => x.Tag!.Name == newTags[1].Name && x.FileId == command.FileId);
        secondFileTag.Should().NotBeNull(); 

        var thirdFileTag = fileTags.FirstOrDefault(x => x.Tag!.Name == newTags[2].Name && x.FileId == command.FileId);
        thirdFileTag.Should().NotBeNull();

        var fourthFileTag = fileTags.FirstOrDefault(x => x.Tag!.Name == newTags[3].Name && x.FileId == command.FileId);
        fourthFileTag.Should().NotBeNull();
        fourthFileTag!.Value.Should().Be(newTags[3].Value);
        
        var fileNotes = context.Notes;
        var firstFileNote = fileNotes.FirstOrDefault(x => x.FileId == command.FileId);
        var expectedFileNote = command.Notes.First();
        firstFileNote.Should().NotBeNull();
        firstFileNote!.SourceId.Should().Be(expectedFileNote.SourceId);
        firstFileNote.Label.Should().Be(expectedFileNote.Label);
        firstFileNote.PositionFromLeft.Should().Be(expectedFileNote.PositionFromLeft);
        firstFileNote.PositionFromTop.Should().Be(expectedFileNote.PositionFromTop);
        firstFileNote.Width.Should().Be(expectedFileNote.Width);
        firstFileNote.Height.Should().Be(expectedFileNote.Height);
        
        harness.Sent
            .AnyMessage<ISavedCommand>(x => x.FileId == command.FileId && x.SourceId == (int)command.MetadataSource)
            .Should().BeTrue();
    }
}