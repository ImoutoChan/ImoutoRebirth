using ImoutoRebirth.Common.Tests;
using ImoutoRebirth.Lilin.WebApi.Client;
using BindTag = ImoutoRebirth.Lilin.WebApi.Client.BindTag;
using BindTagsCommand = ImoutoRebirth.Lilin.WebApi.Client.BindTagsCommand;
using CreateTagCommand = ImoutoRebirth.Lilin.Application.TagSlice.CreateTagCommand;
using MetadataSource = ImoutoRebirth.Lilin.WebApi.Client.MetadataSource;
using TagsSearchQuery = ImoutoRebirth.Lilin.Application.TagSlice.TagsSearchQuery;
using TagValuesSearchQuery = ImoutoRebirth.Lilin.Application.TagSlice.TagValuesSearchQuery;

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
        result.Should().HaveCountGreaterThanOrEqualTo(12);
    }
    
    [Fact]
    public async Task CreateTag()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
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
    public async Task CreateTagShouldUpdateHasValueAndSynonymsWhenTagWithSameNameAndTypeAlreadyExists()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
        var httpClient = _webApp.Client;
        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");

        var existingCreateCommand = new CreateTagCommand(
            types!.First(x => x.Name == "General").Id, 
            "new tag" + Guid.NewGuid(),
            false,
            ["oldSynonym"],
            Domain.TagAggregate.TagOptions.None);
        
        await httpClient.PostAsJsonAsync("/tags", existingCreateCommand).ReadResult<Tag>();

        var createCommand = new CreateTagCommand(
            existingCreateCommand.TypeId, 
            existingCreateCommand.Name,
            true,
            ["newSynonym"],
            Domain.TagAggregate.TagOptions.Counter);

        // act
        var newTag = await httpClient.PostAsJsonAsync("/tags", createCommand).ReadResult<Tag>();
        
        // assert
        var tagEntity = context.Tags.FirstOrDefault(x => x.Name == existingCreateCommand.Name);
        
        newTag.Should().NotBeNull();
        tagEntity.Should().NotBeNull();
        tagEntity!.TypeId.Should().Be(existingCreateCommand.TypeId);
        tagEntity.Options.Should().Be(Domain.TagAggregate.TagOptions.Counter);
        tagEntity.Name.Should().Be(existingCreateCommand.Name);
        tagEntity.SynonymsArray.Should().BeEquivalentTo("oldSynonym", "newSynonym");
        tagEntity.Count.Should().Be(0);
        tagEntity.HasValue.Should().Be(true);
    }
    
    [Fact]
    public async Task CreateTagWithAdditionalInfo()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
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
        var foundTags = await httpClient.PostAsJsonAsync("/tags/search", new TagsSearchQuery("girl", 100))
            .ReadResult<IReadOnlyCollection<Tag>>();
        
        // assert
        foundTags.Should().NotBeNull();
        foundTags.Should().NotBeEmpty();
        foundTags.Should().HaveCountGreaterThanOrEqualTo(3);
        foundTags.Any(x => x.Name!.StartsWith("girl around")).Should().BeTrue();
        foundTags.Any(x => x.Name!.StartsWith("1girl")).Should().BeTrue();
        foundTags.Any(x => x.Name!.StartsWith("2girls")).Should().BeTrue();
        foundTags.Any(x => x.Name!.StartsWith("boy")).Should().BeFalse();
    }

    [Fact]
    public async Task SearchTagsWithEmptyRequest()
    {
        // arrange
        var httpClient = _webApp.Client;
        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");

        await CreateNewTag(httpClient, types, "1girl");
        await CreateNewTag(httpClient, types, "2girls");
        await CreateNewTag(httpClient, types, "girl around");
        await CreateNewTag(httpClient, types, "boy");

        // act
        var foundTags = await httpClient.PostAsJsonAsync("/tags/search", new TagsSearchQuery("", 100))
            .ReadResult<IReadOnlyCollection<Tag>>();
        
        // assert
        foundTags.Should().NotBeNull();
        foundTags.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPopularUserTags()
    {
        // arrange
        var httpClient = _webApp.Client;

        var fileIds = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToList();

        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var newTag1 = await CreateNewTag(httpClient, types, "popular tag");
       
        var requests = fileIds.Select(x => new BindTag(x, MetadataSource.Manual, newTag1.Id, null)).ToList();
        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
            requests, SameTagHandleStrategy.ReplaceExistingValue));
        
        // act
        var foundTags = await httpClient.GetFromJsonAsync<IReadOnlyCollection<Tag>>("/tags/popular?limit=100");
        
        // assert
        foundTags.Should().NotBeNull();
        foundTags.Should().NotBeEmpty();
        var tag = foundTags!.FirstOrDefault(x => x.Id == newTag1.Id);

        tag.Should().NotBeNull();
        tag!.Name.Should().Be(newTag1.Name);
        tag.Count.Should().Be(0);
        tag.Id.Should().Be(newTag1.Id);
        tag.Options.Should().Be(newTag1.Options);
        tag.Synonyms.Should().BeEquivalentTo(newTag1.Synonyms);
        tag.Type!.Id.Should().Be(newTag1.Type!.Id);
        
    }

    [Fact]
    public async Task GetPopularUserCharacterTags()
    {
        // arrange
        var httpClient = _webApp.Client;

        var fileIds = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToList();

        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var newTagCharacter = await CreateNewTag(httpClient, types, "nora fawn", type: "Character");
        var newTagGeneral = await CreateNewTag(httpClient, types, "1girl");

        var requestsCharacter = fileIds
            .Select(x => new BindTag(x, MetadataSource.Manual, newTagCharacter.Id, null))
            .ToList();
        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
            requestsCharacter, SameTagHandleStrategy.ReplaceExistingValue));
       
        var requestsGeneral = fileIds
            .Select(x => new BindTag(x, MetadataSource.Manual, newTagGeneral.Id, null))
            .ToList();
        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
            requestsGeneral, SameTagHandleStrategy.ReplaceExistingValue));
        
        // act
        var foundTags = await httpClient
            .GetFromJsonAsync<IReadOnlyCollection<Tag>>("/tags/popular-characters?limit=100");
        
        // assert
        foundTags.Should().NotBeNull();
        foundTags.Should().NotBeEmpty();
        var tag = foundTags!.First();

        tag.Name.Should().Be(newTagCharacter.Name);
        tag.Count.Should().Be(0);
        tag.Id.Should().Be(newTagCharacter.Id);
        tag.Options.Should().Be(newTagCharacter.Options);
        tag.Synonyms.Should().BeEquivalentTo(newTagCharacter.Synonyms);
        tag.Type!.Id.Should().Be(newTagCharacter.Type!.Id);

        foundTags!.Select(x => x.Id).Should().NotContain(newTagGeneral.Id);
    }
    
    [Fact]
    public async Task MergeTags()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var httpClient = _webApp.Client;
        var context = _webApp.GetDbContext(scope);
        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");

        var tagToClean = await CreateNewTag(httpClient, types, "tag to clean");
        var tagToEnrich = await CreateNewTag(httpClient, types, "tag to enrich");
        
        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();
        var file3Id = Guid.NewGuid();
        var file4Id = Guid.NewGuid();
        var file5Id = Guid.NewGuid();
        
        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
        [
            new(file1Id, MetadataSource.Manual, tagToClean.Id, null),
            new(file2Id, MetadataSource.Manual, tagToClean.Id, null),
            new(file3Id, MetadataSource.Manual, tagToClean.Id, null),
            new(file4Id, MetadataSource.Manual, tagToClean.Id, null),
            new(file5Id, MetadataSource.Manual, tagToClean.Id, null),
            new(file1Id, MetadataSource.Manual, tagToEnrich.Id, null),
            new(file2Id, MetadataSource.Manual, tagToEnrich.Id, null),
            new(file3Id, MetadataSource.Manual, tagToEnrich.Id, null),
            new(file4Id, MetadataSource.Manual, tagToEnrich.Id, null),
            new(file5Id, MetadataSource.Manual, tagToEnrich.Id, null)
            
        ], SameTagHandleStrategy.ReplaceExistingValue));
        
        var tagToCleanCountBefore = context.FileTags.Count(x => x.TagId == tagToClean.Id);
        var tagToEnrichCountBefore = context.FileTags.Count(x => x.TagId == tagToEnrich.Id);
        
        // act
        await httpClient.PostAsJsonAsync($"tags/merge/", new MergeTagsCommand(tagToClean.Id, tagToEnrich.Id));
        
        // assert
        tagToCleanCountBefore.Should().Be(5);
        tagToEnrichCountBefore.Should().Be(5);
        
        var tagToCleanCount = context.FileTags.Count(x => x.TagId == tagToClean.Id);
        tagToCleanCount.Should().Be(0);
        
        var tagToEnrichCount = context.FileTags.Count(x => x.TagId == tagToEnrich.Id);
        tagToEnrichCount.Should().Be(tagToCleanCountBefore + tagToEnrichCountBefore);
    }
    
    [Fact]
    public async Task DeleteTag()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var httpClient = _webApp.Client;
        var context = _webApp.GetDbContext(scope);
        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");

        var tagToClean = await CreateNewTag(httpClient, types, "tag to clean");
        
        // act
        await httpClient.DeleteAsync($"tags/{tagToClean.Id}");
        
        // assert
        var tagToCleanCount = context.Tags.Count(x => x.Id == tagToClean.Id);
        tagToCleanCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchTagValues()
    {
        // arrange

        var httpClient = _webApp.Client;

        var fileIds = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToList();

        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var newTag1 = await CreateNewTag(httpClient, types, "popular tag");

        var requests
                          = fileIds.Take(20).Select((x, i) => new BindTag(x, MetadataSource.Manual, newTag1.Id, "girl" + i))
            .Union(fileIds.Skip(20).Take(20).Select((x, i) => new BindTag(x, MetadataSource.Manual, newTag1.Id, i + "girl")))
            .Union(fileIds.Skip(40).Take(20).Select((x, i) => new BindTag(x, MetadataSource.Manual, newTag1.Id, i + "girl" + i)))
            .Union(fileIds.Skip(60).Take(20).Select((x, i) => new BindTag(x, MetadataSource.Manual, newTag1.Id, "girl")))
            .Union(fileIds.Skip(80).Take(20).Select((x, i) => new BindTag(x, MetadataSource.Manual, newTag1.Id, "1girl")))
            .ToList();

        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
            requests, SameTagHandleStrategy.ReplaceExistingValue));

        // act
        var foundTagValues = await httpClient.PostAsJsonAsync(
                "/tags/values/search",
                new TagValuesSearchQuery(newTag1.Id, "girl", 200))
            .ReadResult<IReadOnlyCollection<string>>();

        // assert
        foundTagValues.Should().NotBeNull();
        foundTagValues.Should().NotBeEmpty();
        foundTagValues.Should().HaveCountGreaterThanOrEqualTo(3);
        foundTagValues.All(x => x.Contains("girl")).Should().BeTrue();
        foundTagValues.Distinct().Count().Should().Be(foundTagValues.Count);
    }
    
    private static async Task<Tag> CreateNewTag(
        HttpClient client,
        IReadOnlyCollection<TagType>? types,
        string namePrefix,
        bool hasValue = false,
        string type = "General")
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
