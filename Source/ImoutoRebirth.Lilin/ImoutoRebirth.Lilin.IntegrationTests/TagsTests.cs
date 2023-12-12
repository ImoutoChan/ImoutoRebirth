﻿using ImoutoRebirth.Common.Tests;
using ImoutoRebirth.LilinService.WebApi.Client;
using BindTag = ImoutoRebirth.LilinService.WebApi.Client.BindTag;
using BindTagsCommand = ImoutoRebirth.LilinService.WebApi.Client.BindTagsCommand;
using CreateTagCommand = ImoutoRebirth.Lilin.Application.TagSlice.CreateTagCommand;
using MetadataSource = ImoutoRebirth.LilinService.WebApi.Client.MetadataSource;
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
        foundTags.Should().HaveCountGreaterOrEqualTo(3);
        foundTags.Any(x => x.Name!.StartsWith("girl around")).Should().BeTrue();
        foundTags.Any(x => x.Name!.StartsWith("1girl")).Should().BeTrue();
        foundTags.Any(x => x.Name!.StartsWith("2girls")).Should().BeTrue();
        foundTags.Any(x => x.Name!.StartsWith("boy")).Should().BeFalse();
    }

    [Fact]
    public async Task GetPopularTags()
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
        var tag = foundTags!.First();

        tag.Name.Should().Be(newTag1.Name);
        tag.Count.Should().Be(0);
        tag.Id.Should().Be(newTag1.Id);
        tag.Options.Should().Be(newTag1.Options);
        tag.Synonyms.Should().BeEquivalentTo(newTag1.Synonyms);
        tag.Type!.Id.Should().Be(newTag1.Type!.Id);
        
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
