using ImoutoRebirth.Common.Tests;
using ImoutoRebirth.LilinService.WebApi.Client;
using CreateTagCommand = ImoutoRebirth.Lilin.Application.TagSlice.CreateTagCommand;

namespace ImoutoRebirth.Lilin.IntegrationTests;

[Collection("WebApplication")]
public class FileTagsTests(TestWebApplicationFactory<Program> _webApp)
{
    [Fact]
    public async Task BindTags()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;

        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();
        var file3Id = Guid.NewGuid();
        var file4Id = Guid.NewGuid();
        var file5Id = Guid.NewGuid();

        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var newTag1 = await CreateNewTag(httpClient, types, "1girl");
        var newTag2 = await CreateNewTag(httpClient, types, "2girl");
        
        // act
        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
        [
            new(file1Id, MetadataSource.Manual, newTag1.Id, null),
            new(file1Id, MetadataSource.Manual, newTag2.Id, null),
            new(file2Id, MetadataSource.Manual, newTag1.Id, null),
            new(file2Id, MetadataSource.Manual, newTag2.Id, null),
            new(file3Id, MetadataSource.Manual, newTag1.Id, null),
            new(file3Id, MetadataSource.Manual, newTag2.Id, null),
            new(file4Id, MetadataSource.Manual, newTag1.Id, null),
            new(file4Id, MetadataSource.Manual, newTag2.Id, null),
            new(file5Id, MetadataSource.Manual, newTag1.Id, null),
            new(file5Id, MetadataSource.Manual, newTag2.Id, null),
        ], SameTagHandleStrategy.ReplaceExistingValue));
        
        // assert
        context.FileTags.Where(x => x.FileId == file1Id).Should().HaveCount(2);
        context.FileTags.Where(x => x.FileId == file2Id).Should().HaveCount(2);
        context.FileTags.Where(x => x.FileId == file3Id).Should().HaveCount(2);
        context.FileTags.Where(x => x.FileId == file4Id).Should().HaveCount(2);
        context.FileTags.Where(x => x.FileId == file5Id).Should().HaveCount(2);
        context.FileTags.Where(x => x.TagId == newTag1.Id).Should().HaveCount(5);
        context.FileTags.Where(x => x.TagId == newTag2.Id).Should().HaveCount(5);
    }

    [Fact]
    public async Task SearchFilesFast()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
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
        var found1Girl = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag1.Id, TagSearchScope.Included, null)]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        var foundSolo = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag3.Id, TagSearchScope.Included, null)]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        var foundBlueHair = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag4.Id, TagSearchScope.Included, null)]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        var found2Girl = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag2.Id, TagSearchScope.Included, null)]))
            .ReadResult<IReadOnlyCollection<Guid>>();

        var found1GirlBlueHair = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([
                    new(newTag4.Id, TagSearchScope.Included, null),
                    new(newTag1.Id, TagSearchScope.Included, null)
                ]))
            .ReadResult<IReadOnlyCollection<Guid>>();

        var found1GirlExcludeBlueHair = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([
                    new(newTag4.Id, TagSearchScope.Excluded, null),
                    new(newTag1.Id, TagSearchScope.Included, null)
                ]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        // assert
        found1Girl.Should().HaveCount(2);
        found1Girl.Should().BeEquivalentTo([file1Id, file3Id]);
        foundSolo.Should().HaveCount(3);
        foundSolo.Should().BeEquivalentTo([file1Id, file3Id, file4Id]);
        foundBlueHair.Should().HaveCount(4);
        foundBlueHair.Should().BeEquivalentTo([file1Id, file2Id, file4Id, file5Id]);
        found1GirlBlueHair.Should().HaveCount(1);
        found1GirlBlueHair.Should().BeEquivalentTo([file1Id]);
        found2Girl.Should().HaveCount(2);
        found2Girl.Should().BeEquivalentTo([file2Id, file5Id]);
        found1GirlExcludeBlueHair.Should().HaveCount(1);
        found1GirlExcludeBlueHair.Should().BeEquivalentTo([file3Id]);
    }
    
    [Fact]
    public async Task BindTagsShouldReplaceExistingValues()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;

        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();

        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var newTag1 = await CreateNewTag(httpClient, types, "rating");
        var newTag2 = await CreateNewTag(httpClient, types, "quality");

        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
        [
            new(file1Id, MetadataSource.Manual, newTag1.Id, "safe"),
            new(file1Id, MetadataSource.Manual, newTag2.Id, "good"),
            new(file2Id, MetadataSource.Manual, newTag1.Id, "questionable"),
            new(file2Id, MetadataSource.Manual, newTag2.Id, "bad"),
        ], SameTagHandleStrategy.ReplaceExistingValue));
        
        // act
        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
        [
            new(file1Id, MetadataSource.Manual, newTag1.Id, "safe edited"),
            new(file1Id, MetadataSource.Manual, newTag2.Id, "good edited"),
            new(file2Id, MetadataSource.Manual, newTag1.Id, "questionable edited"),
            new(file2Id, MetadataSource.Manual, newTag2.Id, "bad edited"),
        ], SameTagHandleStrategy.ReplaceExistingValue));
        
        // assert
        context.FileTags.Where(x => x.FileId == file1Id).Should().HaveCount(2);
        context.FileTags.Where(x => x.FileId == file2Id).Should().HaveCount(2);
        context.FileTags.First(x => x.FileId == file1Id && x.TagId == newTag1.Id)
            .Value.Should().Be("safe edited");
        context.FileTags.First(x => x.FileId == file1Id && x.TagId == newTag2.Id)
            .Value.Should().Be("good edited");
        context.FileTags.First(x => x.FileId == file2Id && x.TagId == newTag1.Id)
            .Value.Should().Be("questionable edited");
        context.FileTags.First(x => x.FileId == file2Id && x.TagId == newTag2.Id)
            .Value.Should().Be("bad edited");
    }
    
    [Fact]
    public async Task BindTagsShouldNotReplaceExistingValues()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;

        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();

        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var newTag1 = await CreateNewTag(httpClient, types, "rating");
        var newTag2 = await CreateNewTag(httpClient, types, "quality");

        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
        [
            new(file1Id, MetadataSource.Manual, newTag1.Id, "safe"),
            new(file1Id, MetadataSource.Manual, newTag2.Id, "good"),
            new(file2Id, MetadataSource.Manual, newTag1.Id, "questionable"),
            new(file2Id, MetadataSource.Manual, newTag2.Id, "bad"),
        ], SameTagHandleStrategy.ReplaceExistingValue));
        
        // act
        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
        [
            new(file1Id, MetadataSource.Manual, newTag1.Id, "safe edited"),
            new(file1Id, MetadataSource.Manual, newTag2.Id, "good edited"),
            new(file2Id, MetadataSource.Manual, newTag1.Id, "questionable edited"),
            new(file2Id, MetadataSource.Manual, newTag2.Id, "bad edited"),
        ], SameTagHandleStrategy.AddNewFileTag));
        
        // assert
        context.FileTags.Where(x => x.FileId == file1Id).Should().HaveCount(4);
        context.FileTags.Where(x => x.FileId == file2Id).Should().HaveCount(4);
        context.FileTags.Where(x => x.FileId == file1Id && x.TagId == newTag1.Id)
            .Any(x => x.Value == "safe edited").Should().BeTrue();
        context.FileTags.Where(x => x.FileId == file1Id && x.TagId == newTag1.Id)
            .Any(x => x.Value == "safe").Should().BeTrue();
        context.FileTags.Where(x => x.FileId == file1Id && x.TagId == newTag2.Id)
            .Any(x => x.Value == "good edited").Should().BeTrue();
        context.FileTags.Where(x => x.FileId == file1Id && x.TagId == newTag2.Id)
            .Any(x => x.Value == "good").Should().BeTrue();
        context.FileTags.Where(x => x.FileId == file2Id && x.TagId == newTag1.Id)
            .Any(x => x.Value == "questionable edited").Should().BeTrue();
        context.FileTags.Where(x => x.FileId == file2Id && x.TagId == newTag1.Id)
            .Any(x => x.Value == "questionable").Should().BeTrue();
    }
    
    [Fact]
    public async Task BindEmptyTagsShouldWork()
    {
        // arrange
        var httpClient = _webApp.Client;
        
        // act
        var result = await httpClient
            .PostAsJsonAsync("/files/tags", new BindTagsCommand([], SameTagHandleStrategy.ReplaceExistingValue));
        
        // assert
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task UnbindTagsCommand()
    {
        // arrange
        var context = _webApp.GetDbContext(_webApp.GetScope());
        var httpClient = _webApp.Client;

        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();

        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var newTag1 = await CreateNewTag(httpClient, types, "rating");
        var newTag2 = await CreateNewTag(httpClient, types, "quality");

        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
        [
            new(file1Id, MetadataSource.Manual, newTag1.Id, "safe"),
            new(file1Id, MetadataSource.Manual, newTag2.Id, "good"),
            new(file2Id, MetadataSource.Manual, newTag1.Id, "questionable"),
            new(file2Id, MetadataSource.Manual, newTag2.Id, "bad"),
        ], SameTagHandleStrategy.ReplaceExistingValue));
        
        // act
        await httpClient.SendAsync(new(
            HttpMethod.Delete,
            "/files/tags")
        {
            Content = JsonContent.Create(new UnbindTagsCommand(
            [
                new(file1Id, MetadataSource.Manual, newTag1.Id, "safe"),
                new(file1Id, MetadataSource.Manual, newTag2.Id, "good"),
                new(file2Id, MetadataSource.Manual, newTag1.Id, "questionable"),
            ]))
        });
        
        // assert
        context.FileTags.Where(x => x.FileId == file1Id).Should().HaveCount(0);
        context.FileTags.Where(x => x.FileId == file2Id).Should().HaveCount(1);
        context.FileTags.FirstOrDefault(x => x.FileId == file1Id).Should().BeNull();
        context.FileTags.FirstOrDefault(x => x.FileId == file1Id).Should().BeNull();
        context.FileTags.FirstOrDefault(x => x.FileId == file2Id && x.TagId == newTag1.Id).Should().BeNull();
        context.FileTags.First(x => x.FileId == file2Id && x.TagId == newTag2.Id).Value.Should().Be("bad");
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
