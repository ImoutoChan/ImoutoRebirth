using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Commands;
using ImoutoRebirth.Lilin.Domain.TagAggregate;
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
        using var scope = _webApp.GetScope();
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
            Domain.FileInfoAggregate.MetadataSource.Danbooru,
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
    
    [Fact]
    public async Task ActualizeFileInfoForSourceCommandShouldUpdateNote()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var context = _webApp.GetDbContext(scope);

        var noteVersion1 = new ActualizeNote(
            SourceId: 1,
            Label: "Bounce sound translation" + Guid.NewGuid(),
            PositionFromLeft: 10,
            PositionFromTop: 20,
            Width: 30,
            Height: 40);

        var fileId = Guid.NewGuid();
        
        var command = new ActualizeFileInfoForSourceCommand(
            fileId,
            Domain.FileInfoAggregate.MetadataSource.Danbooru,
            Array.Empty<ActualizeTag>(),
            [noteVersion1]);
        await mediator.Send(command);
        
        // act
        var noteVersion2 = new ActualizeNote(
            SourceId: 1,
            Label: "Bounce sound translation version 2" + Guid.NewGuid(),
            PositionFromLeft: 11,
            PositionFromTop: 21,
            Width: 31,
            Height: 41);
        
        var command2 = new ActualizeFileInfoForSourceCommand(
            fileId,
            Domain.FileInfoAggregate.MetadataSource.Danbooru,
            Array.Empty<ActualizeTag>(),
            [noteVersion2]);
        await mediator.Send(command2);
        

        // assert
        var fileNotes = context.Notes.Where(x => x.FileId == command.FileId).ToList();
        fileNotes.Should().HaveCount(1);
        var firstFileNote = fileNotes.First();
        
        var expectedFileNote = noteVersion2;
        firstFileNote.Should().NotBeNull();
        firstFileNote!.SourceId.Should().Be(expectedFileNote.SourceId);
        firstFileNote.Label.Should().Be(expectedFileNote.Label);
        firstFileNote.PositionFromLeft.Should().Be(expectedFileNote.PositionFromLeft);
        firstFileNote.PositionFromTop.Should().Be(expectedFileNote.PositionFromTop);
        firstFileNote.Width.Should().Be(expectedFileNote.Width);
        firstFileNote.Height.Should().Be(expectedFileNote.Height);
        
        harness.Sent
            .SelectMessages<ISavedCommand>()
            .Where(x => x.FileId == command.FileId && x.SourceId == (int)command.MetadataSource)
            .Should()
            .HaveCount(2);
    }
    
    [Fact]
    public async Task ActualizeFileInfoForSourceCommandShouldRemoveObsoleteNote()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var context = _webApp.GetDbContext(scope);

        var noteVersion1 = new ActualizeNote(
            SourceId: 1,
            Label: "Bounce sound translation" + Guid.NewGuid(),
            PositionFromLeft: 10,
            PositionFromTop: 20,
            Width: 30,
            Height: 40);

        var fileId = Guid.NewGuid();
        
        var command = new ActualizeFileInfoForSourceCommand(
            fileId,
            Domain.FileInfoAggregate.MetadataSource.Danbooru,
            Array.Empty<ActualizeTag>(),
            [noteVersion1]);
        await mediator.Send(command);
        
        // act
        var noteVersion2 = new ActualizeNote(
            SourceId: 2,
            Label: "Bounce sound translation NEW 2" + Guid.NewGuid(),
            PositionFromLeft: 11,
            PositionFromTop: 21,
            Width: 31,
            Height: 41);
        
        var command2 = new ActualizeFileInfoForSourceCommand(
            fileId,
            Domain.FileInfoAggregate.MetadataSource.Danbooru,
            Array.Empty<ActualizeTag>(),
            [noteVersion2]);
        await mediator.Send(command2);
        

        // assert
        var fileNotes = context.Notes.Where(x => x.FileId == command.FileId).ToList();
        fileNotes.Should().HaveCount(1);
        var firstFileNote = fileNotes.First();
        
        var expectedFileNote = noteVersion2;
        firstFileNote.Should().NotBeNull();
        firstFileNote!.SourceId.Should().Be(expectedFileNote.SourceId);
        firstFileNote.Label.Should().Be(expectedFileNote.Label);
        firstFileNote.PositionFromLeft.Should().Be(expectedFileNote.PositionFromLeft);
        firstFileNote.PositionFromTop.Should().Be(expectedFileNote.PositionFromTop);
        firstFileNote.Width.Should().Be(expectedFileNote.Width);
        firstFileNote.Height.Should().Be(expectedFileNote.Height);
        
        harness.Sent
            .SelectMessages<ISavedCommand>()
            .Where(x => x.FileId == command.FileId && x.SourceId == (int)command.MetadataSource)
            .Should()
            .HaveCount(2);
    }

    [Fact]
    public async Task ActualizeFileInfoForSourceCommandShouldCreateNewTagTypes()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var context = _webApp.GetDbContext(scope);

        ActualizeTag[] newTags =
        [
            new("General" + Guid.NewGuid(), "1girl" + Guid.NewGuid(), null, null, Domain.TagAggregate.TagOptions.None),
            new("General" + Guid.NewGuid(), "solo" + Guid.NewGuid(), null, null, Domain.TagAggregate.TagOptions.None),
            new("General" + Guid.NewGuid(), "huge melons" + Guid.NewGuid(), null, null, Domain.TagAggregate.TagOptions.None),
            new("General" + Guid.NewGuid(), "Rating" + Guid.NewGuid(), "Questionable", new[] { "Rate" }, Domain.TagAggregate.TagOptions.None),
        ];
        
        // act
        var command = new ActualizeFileInfoForSourceCommand(
            Guid.NewGuid(),
            Domain.FileInfoAggregate.MetadataSource.Danbooru,
            newTags,
            Array.Empty<ActualizeNote>());

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
    }
    

    [Fact]
    public async Task ActualizeFileInfoForSourceCommandShouldUpdateTagParams()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var context = _webApp.GetDbContext(scope);
        
        var command = new ActualizeFileInfoForSourceCommand(
            Guid.NewGuid(),
            Domain.FileInfoAggregate.MetadataSource.Danbooru,
            [new("General" + Guid.NewGuid(), "1girl" + Guid.NewGuid(), null, null, Domain.TagAggregate.TagOptions.None)],
            Array.Empty<ActualizeNote>());

        await mediator.Send(command);
        
        // act
        var command2 = new ActualizeFileInfoForSourceCommand(
            Guid.NewGuid(),
            Domain.FileInfoAggregate.MetadataSource.Danbooru,
            [command.Tags.First() with
            {
                Options = TagOptions.Counter, 
                Value = "Counter:1", 
                Synonyms = ["1woman"]
            }],
            Array.Empty<ActualizeNote>());

        await mediator.Send(command2);

        // assert
        var tags = context.Tags.Include(tagEntity => tagEntity.Type!);
        
        var firstTag = tags.FirstOrDefault(x => x.Name == command.Tags.First().Name);
        var expected = command2.Tags.First();
        firstTag.Should().NotBeNull();
        firstTag!.Type!.Name.Should().Be(expected.Type);
        firstTag!.Options.Should().Be(expected.Options);
        firstTag!.HasValue.Should().Be(true);
        firstTag!.SynonymsArray.Should().BeEquivalentTo(expected.Synonyms);
    }
}
