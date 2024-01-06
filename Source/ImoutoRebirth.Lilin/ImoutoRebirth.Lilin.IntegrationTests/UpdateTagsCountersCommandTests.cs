using ImoutoRebirth.Common.Tests;
using ImoutoRebirth.Lilin.Application.TagSlice;
using ImoutoRebirth.Lilin.WebApi.Client;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using CreateTagCommand = ImoutoRebirth.Lilin.Application.TagSlice.CreateTagCommand;

namespace ImoutoRebirth.Lilin.IntegrationTests;

[Collection("WebApplication")]
public class UpdateTagsCountersCommandTests(TestWebApplicationFactory<Program> _webApp)
{
    [Fact]
    public async Task UpdateTagsCountersCommand()
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
        var newTag1 = await CreateNewTag(httpClient, types, "1girl");
        var newTag2 = await CreateNewTag(httpClient, types, "2girl");
        var newTag3 = await CreateNewTag(httpClient, types, "solo");
        var newTag4 = await CreateNewTag(httpClient, types, "blue hair");

        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
        [
            new(file1Id, MetadataSource.Manual, newTag1.Id, null),
            new(file1Id, MetadataSource.Manual, newTag3.Id, null),
            new(file1Id, MetadataSource.Manual, newTag4.Id, null),

            new(file2Id, MetadataSource.Manual, newTag2.Id, null),
            new(file2Id, MetadataSource.Manual, newTag4.Id, null),
            
            new(file3Id, MetadataSource.Manual, newTag1.Id, null),
            new(file3Id, MetadataSource.Manual, newTag3.Id, null),
            
            new(file4Id, MetadataSource.Manual, newTag3.Id, null),
            new(file4Id, MetadataSource.Manual, newTag4.Id, null),

            new(file5Id, MetadataSource.Manual, newTag2.Id, null),
        ], SameTagHandleStrategy.ReplaceExistingValue));
        
        // act
        await mediator.Send(new UpdateTagsCountersCommand());

        // assert
        var tag1 = context.Tags.First(x => x.Id == newTag1.Id);
        tag1.Count.Should().Be(2);
        var tag2 = context.Tags.First(x => x.Id == newTag2.Id);
        tag2.Count.Should().Be(2);
        var tag3 = context.Tags.First(x => x.Id == newTag3.Id);
        tag3.Count.Should().Be(3);
        var tag4 = context.Tags.First(x => x.Id == newTag4.Id);
        tag4.Count.Should().Be(3);
    }
    
    private static async Task<Tag> CreateNewTag(
        HttpClient client,
        IReadOnlyCollection<TagType>? types,
        string namePrefix,
        bool hasValue = false)
    {
        var typeId = types!.First(x => x.Name == "General").Id;
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
