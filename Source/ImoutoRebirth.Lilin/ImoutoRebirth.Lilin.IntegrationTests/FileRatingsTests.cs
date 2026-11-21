using MediatR;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.IntegrationTests;

[Collection("WebApplication")]
public class FileRatingsTests(TestWebApplicationFactory<Program> webApp)
{
    [Fact]
    public async Task Handle_ShouldReturnOnlyRatingsAndFavoritesForRequestedFiles()
    {
        using var scope = webApp.GetScope();
        var db = webApp.GetDbContext(scope);
        await using var transaction = await db.Database.BeginTransactionAsync();
        var typeId = await db.TagTypes.Select(x => x.Id).FirstAsync();
        async Task<TagEntity> Tag(string name) => await db.Tags.FirstOrDefaultAsync(x => x.Name == name)
            ?? new TagEntity { Id = Guid.NewGuid(), TypeId = typeId, Name = name };
        var favorite = await Tag("Favorite");
        var rate = await Tag("Rate");
        var rated = Guid.NewGuid();
        var invalid = Guid.NewGuid();
        var outside = Guid.NewGuid();
        db.FileTags.AddRange(
            new FileTagEntity { Id = Guid.NewGuid(), FileId = rated, Tag = favorite },
            new FileTagEntity { Id = Guid.NewGuid(), FileId = rated, Tag = rate, Value = "4" },
            new FileTagEntity { Id = Guid.NewGuid(), FileId = invalid, Tag = rate, Value = "invalid" },
            new FileTagEntity { Id = Guid.NewGuid(), FileId = outside, Tag = favorite });
        await db.SaveChangesAsync();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var ratings = await mediator.Send(new FileRatingsQuery([rated, invalid, rated, Guid.NewGuid()]));

        ratings.Should().BeEquivalentTo([new FileRating(rated, true, 4), new FileRating(invalid, false, 0)]);
    }
}
