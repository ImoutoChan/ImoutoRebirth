using ImoutoRebirth.Common.Tests;
using ImoutoRebirth.Lilin.Application.TagSlice;
using ImoutoRebirth.Lilin.WebApi.Client;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using CreateTagCommand = ImoutoRebirth.Lilin.Application.TagSlice.CreateTagCommand;

namespace ImoutoRebirth.Lilin.IntegrationTests;

[Collection("WebApplication")]
public class DeleteEmptyLocationTagsCommandTests(TestWebApplicationFactory<Program> _webApp)
{
    [Fact]
    public async Task DeleteEmptyLocationTagsCommand()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var context = _webApp.GetDbContext(scope);
        var httpClient = _webApp.Client;
        
        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();
        var file3Id = Guid.NewGuid();
        var file4Id = Guid.NewGuid();
        var file5Id = Guid.NewGuid();

        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var newTag1 = await CreateNewTag(httpClient, "subfolder1", types, "Location");
        var newTag2 = await CreateNewTag(httpClient, "subfolder2", types, "Location");
        var newTag3 = await CreateNewTag(httpClient, "subfolder3", types, "Location");
        var newTag4 = await CreateNewTag(httpClient, "subfolder4", types, "Location");
        var newTag5 = await CreateNewTag(httpClient, "filename", types, "Location");

        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
        [
            new(file1Id, MetadataSource.Manual, newTag1.Id, null),
            new(file1Id, MetadataSource.Manual, newTag3.Id, null),
            new(file1Id, MetadataSource.Manual, newTag4.Id, null),

            new(file2Id, MetadataSource.Manual, newTag4.Id, null),
            
            new(file3Id, MetadataSource.Manual, newTag1.Id, null),
            new(file3Id, MetadataSource.Manual, newTag3.Id, null),
            
            new(file4Id, MetadataSource.Manual, newTag3.Id, null),
            new(file4Id, MetadataSource.Manual, newTag4.Id, null),

            new(file5Id, MetadataSource.Manual, newTag5.Id, null),
        ], SameTagHandleStrategy.ReplaceExistingValue));
        
        // act
        await mediator.Send(new DeleteEmptyLocationTagsCommand());

        // assert
        context.Tags.Any(x => x.Id == newTag1.Id).Should().BeTrue();
        context.Tags.Any(x => x.Id == newTag2.Id).Should().BeFalse();
        context.Tags.Any(x => x.Id == newTag3.Id).Should().BeTrue();
        context.Tags.Any(x => x.Id == newTag4.Id).Should().BeTrue();
        context.Tags.Any(x => x.Id == newTag5.Id).Should().BeTrue();
    }
    
    private static async Task<Tag> CreateNewTag(
        HttpClient client,
        string namePrefix,
        IReadOnlyCollection<TagType>? types,
        string type = "General",
        bool hasValue = false)
    {
        var typeId = types!.First(x => x.Name == type).Id;
        return await client
            .PostAsJsonAsync(
                "/tags",
                new CreateTagCommand(
                    typeId,
                    namePrefix + Guid.NewGuid(),
                    hasValue,
                    [],
                    Domain.TagAggregate.TagOptions.None))
            .ReadResult<Tag>();
    }
}
