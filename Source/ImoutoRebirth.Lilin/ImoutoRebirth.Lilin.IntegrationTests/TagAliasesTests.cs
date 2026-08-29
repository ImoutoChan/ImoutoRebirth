using ImoutoRebirth.Common.Tests;
using ImoutoRebirth.Lilin.WebApi.Client;
using CreateTagCommand = ImoutoRebirth.Lilin.Application.TagSlice.CreateTagCommand;
using SetTagAliasesCommand = ImoutoRebirth.Lilin.Application.TagSlice.SetTagAliasesCommand;

namespace ImoutoRebirth.Lilin.IntegrationTests;

[Collection("WebApplication")]
public class TagAliasesTests(TestWebApplicationFactory<Program> _webApp)
{
    [Fact]
    public async Task SetTagAliasesShouldCreateAliasesReadableFromBothSides()
    {
        // arrange
        var httpClient = _webApp.Client;
        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var tagA = await CreateNewTag(httpClient, types, "1girl");
        var tagB = await CreateNewTag(httpClient, types, "solo");
        var tagC = await CreateNewTag(httpClient, types, "1woman");

        // act
        await httpClient.PostAsJsonAsync("/tags/aliases", new SetTagAliasesCommand(tagA.Id, [tagB.Id, tagC.Id]));

        // assert
        var aliasesOfA = await GetAliases(httpClient, tagA.Id);
        var aliasesOfB = await GetAliases(httpClient, tagB.Id);
        var aliasesOfC = await GetAliases(httpClient, tagC.Id);

        aliasesOfA.Select(x => x.Id).Should().BeEquivalentTo([tagB.Id, tagC.Id]);
        aliasesOfB.Select(x => x.Id).Should().BeEquivalentTo([tagA.Id]);
        aliasesOfC.Select(x => x.Id).Should().BeEquivalentTo([tagA.Id]);
    }

    [Fact]
    public async Task SetTagAliasesShouldReplaceExistingAliases()
    {
        // arrange
        var httpClient = _webApp.Client;
        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var tagA = await CreateNewTag(httpClient, types, "1girl");
        var tagB = await CreateNewTag(httpClient, types, "solo");
        var tagC = await CreateNewTag(httpClient, types, "1woman");

        await httpClient.PostAsJsonAsync("/tags/aliases", new SetTagAliasesCommand(tagA.Id, [tagB.Id, tagC.Id]));

        // act
        await httpClient.PostAsJsonAsync("/tags/aliases", new SetTagAliasesCommand(tagA.Id, [tagB.Id]));

        // assert
        var aliasesOfA = await GetAliases(httpClient, tagA.Id);
        var aliasesOfC = await GetAliases(httpClient, tagC.Id);

        aliasesOfA.Select(x => x.Id).Should().BeEquivalentTo([tagB.Id]);
        aliasesOfC.Should().BeEmpty();
    }

    [Fact]
    public async Task SetTagAliasesWithEmptySetShouldRemoveAllAliases()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
        var httpClient = _webApp.Client;
        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var tagA = await CreateNewTag(httpClient, types, "1girl");
        var tagB = await CreateNewTag(httpClient, types, "solo");

        await httpClient.PostAsJsonAsync("/tags/aliases", new SetTagAliasesCommand(tagA.Id, [tagB.Id]));

        // act
        await httpClient.PostAsJsonAsync("/tags/aliases", new SetTagAliasesCommand(tagB.Id, []));

        // assert
        var aliasesOfA = await GetAliases(httpClient, tagA.Id);
        var aliasesOfB = await GetAliases(httpClient, tagB.Id);

        aliasesOfA.Should().BeEmpty();
        aliasesOfB.Should().BeEmpty();
        context.TagAliases.Where(x => x.TagId == tagA.Id || x.AliasTagId == tagA.Id).Should().BeEmpty();
    }

    [Fact]
    public async Task TagCanBeAliasOfMultipleTags()
    {
        // arrange
        var httpClient = _webApp.Client;
        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var tagA = await CreateNewTag(httpClient, types, "1girl");
        var tagB = await CreateNewTag(httpClient, types, "solo");
        var tagC = await CreateNewTag(httpClient, types, "1woman");

        // act
        await httpClient.PostAsJsonAsync("/tags/aliases", new SetTagAliasesCommand(tagA.Id, [tagB.Id]));
        await httpClient.PostAsJsonAsync("/tags/aliases", new SetTagAliasesCommand(tagC.Id, [tagB.Id]));

        // assert
        var aliasesOfA = await GetAliases(httpClient, tagA.Id);
        var aliasesOfB = await GetAliases(httpClient, tagB.Id);
        var aliasesOfC = await GetAliases(httpClient, tagC.Id);

        aliasesOfA.Select(x => x.Id).Should().BeEquivalentTo([tagB.Id]);
        aliasesOfB.Select(x => x.Id).Should().BeEquivalentTo([tagA.Id, tagC.Id]);
        aliasesOfC.Select(x => x.Id).Should().BeEquivalentTo([tagB.Id]);
    }

    [Fact]
    public async Task SetTagAliasesShouldIgnoreSelfAndDuplicates()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
        var httpClient = _webApp.Client;
        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var tagA = await CreateNewTag(httpClient, types, "1girl");
        var tagB = await CreateNewTag(httpClient, types, "solo");

        // act
        await httpClient.PostAsJsonAsync(
            "/tags/aliases",
            new SetTagAliasesCommand(tagA.Id, [tagA.Id, tagB.Id, tagB.Id]));

        // assert
        var aliasesOfA = await GetAliases(httpClient, tagA.Id);

        aliasesOfA.Select(x => x.Id).Should().BeEquivalentTo([tagB.Id]);
        context.TagAliases.Where(x => x.TagId == tagA.Id || x.AliasTagId == tagA.Id).Should().HaveCount(1);
    }

    private static async Task<IReadOnlyCollection<Tag>> GetAliases(HttpClient client, Guid tagId)
        => (await client.GetFromJsonAsync<IReadOnlyCollection<Tag>>($"/tags/{tagId}/aliases"))!;

    private static async Task<Tag> CreateNewTag(
        HttpClient client,
        IReadOnlyCollection<TagType>? types,
        string namePrefix)
    {
        var typeId = types!.First(x => x.Name == "General").Id;
        return await client
            .PostAsJsonAsync(
                "/tags",
                new CreateTagCommand(
                    typeId,
                    namePrefix + Guid.NewGuid(),
                    false,
                    [],
                    Domain.TagAggregate.TagOptions.None))
            .ReadResult<Tag>();
    }
}
