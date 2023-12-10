using ImoutoRebirth.Common.Tests;
using ImoutoRebirth.LilinService.WebApi.Client;
using CreateTagCommand = ImoutoRebirth.Lilin.Application.TagSlice.CreateTagCommand;
using JsonSerializer = System.Text.Json.JsonSerializer;
using TagsSearchQuery = ImoutoRebirth.Lilin.Application.TagSlice.TagsSearchQuery;

namespace ImoutoRebirth.Lilin.IntegrationTests;

[Collection("WebApplication")]
public class TagsTests(TestWebApplicationFactory<Program> _webApp)
{
    [Fact]
    public async Task GetTagTypes()
    {
        // arrange
        var httpClient = _webApp.Client;

        // act
        var result = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        
        // assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().HaveCount(12);
    }
    
    [Fact]
    public async Task CreateTag()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;
        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");

        var command = new CreateTagCommand(
            types!.First(x => x.Name == "General").Id, 
            "new tag" + Guid.NewGuid(),
            false, 
            [],
            Domain.TagAggregate.TagOptions.None);

        // act
        var result = await httpClient.PostAsJsonAsync("/tags", command);
        
        // assert
        var tagEntity = context.Tags.FirstOrDefault(x => x.Name == command.Name);
        
        result.Should().NotBeNull();
        tagEntity.Should().NotBeNull();
        tagEntity!.TypeId.Should().Be(command.TypeId);
        tagEntity.Options.Should().Be(command.Options);
        tagEntity.Name.Should().Be(command.Name);
        tagEntity.SynonymsArray.Should().BeEquivalentTo(command.Synonyms);
        tagEntity.Count.Should().Be(0);
        tagEntity.HasValue.Should().Be(false);
    }
    
    [Fact]
    public async Task CreateTagWithAdditionalInfo()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;
        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");

        var command = new CreateTagCommand(
            types!.First(x => x.Name == "General").Id, 
            "new tag" + Guid.NewGuid(),
            true, 
            ["Synonyms"],
            Domain.TagAggregate.TagOptions.Counter);

        // act
        var result = await httpClient.PostAsJsonAsync("/tags", command);
        
        // assert
        var tagEntity = context.Tags.FirstOrDefault(x => x.Name == command.Name);
        
        result.Should().NotBeNull();
        tagEntity.Should().NotBeNull();
        tagEntity!.TypeId.Should().Be(command.TypeId);
        tagEntity.Options.Should().Be(command.Options);
        tagEntity.Name.Should().Be(command.Name);
        tagEntity.SynonymsArray.Should().BeEquivalentTo(command.Synonyms);
        tagEntity.Count.Should().Be(0);
        tagEntity.HasValue.Should().Be(command.HasValue);
    }
    
    [Fact]
    public async Task SearchTags()
    {
        // arrange
        var httpClient = _webApp.Client;
        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");

        await CreateNewTag(httpClient, types, "1girl");
        await CreateNewTag(httpClient, types, "2girls");
        await CreateNewTag(httpClient, types, "girl around");
        await CreateNewTag(httpClient, types, "boy");

        // act
        var foundTags = await httpClient.PostAsJsonAsync("/tags/search", new TagsSearchQuery("girl", 10))
            .ReadResult<IReadOnlyCollection<Tag>>();
        
        // assert
        foundTags.Should().NotBeNull();
        foundTags.Should().NotBeEmpty();
        foundTags.Should().HaveCountGreaterOrEqualTo(3);
        foundTags.Any(x => x.Name!.StartsWith("girl around")).Should().BeTrue();
        foundTags.Any(x => x.Name!.StartsWith("1girl")).Should().BeTrue();
        foundTags.Any(x => x.Name!.StartsWith("2girls")).Should().BeTrue();
        foundTags.Any(x => x.Name!.StartsWith("boy")).Should().BeFalse();
    }

    private static async Task CreateNewTag(HttpClient client, IReadOnlyCollection<TagType>? types, string namePrefix)
    {
        var typeId = types!.First(x => x.Name == "General").Id;
        await client.PostAsJsonAsync(
            "/tags",
            new CreateTagCommand(
                typeId, 
                namePrefix + Guid.NewGuid(), 
                false, 
                [],
                Domain.TagAggregate.TagOptions.None));

    }
}
