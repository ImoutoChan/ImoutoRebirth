using ImoutoRebirth.Common.Tests;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Commands;
using ImoutoRebirth.Lilin.WebApi.Client;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using BindTagsCommand = ImoutoRebirth.Lilin.WebApi.Client.BindTagsCommand;
using CreateTagCommand = ImoutoRebirth.Lilin.Application.TagSlice.CreateTagCommand;
using UnbindTagsCommand = ImoutoRebirth.Lilin.WebApi.Client.UnbindTagsCommand;
using AppRelativeShortInfo = ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries.RelativeShortInfo;
using AppRelativeType = ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries.RelativeType;

namespace ImoutoRebirth.Lilin.IntegrationTests;

[Collection("WebApplication")]
public class FileTagsTests(TestWebApplicationFactory<Program> _webApp)
{
    private const string ImageHash1 = "39ddea76d926a396e1f3c2dc6caaa2b1";
    private const string ImageHash2 = "49ddea76d926a396e1f3c2dc6caaa2b1";
    private const string ImageHash3 = "59ddea76d926a396e1f3c2dc6caaa2b1";
    private const string ImageHash4 = "39ddea76d926a396e1f3c2dc6caaa2be";
    private const string ImageHash5 = "49ddea76d926a396e1f3c2dc6caaa2be";
    private const string ImageHash6 = "69ddea76d926a396e1f3c2dc6caaa2be";

    [Fact]
    public async Task BindTags()
    {
        // arrange
        using var scope = _webApp.GetScope();
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
        foundBlueHair.Should().HaveCount(3);
        foundBlueHair.Should().BeEquivalentTo([file1Id, file2Id, file4Id]);
        found1GirlBlueHair.Should().HaveCount(1);
        found1GirlBlueHair.Should().BeEquivalentTo([file1Id]);
        found2Girl.Should().HaveCount(2);
        found2Girl.Should().BeEquivalentTo([file2Id, file5Id]);
        found1GirlExcludeBlueHair.Should().HaveCount(1);
        found1GirlExcludeBlueHair.Should().BeEquivalentTo([file3Id]);
    }

    [Fact]
    public async Task SearchFilesFastWithOnlyExcludedTags()
    {
        // arrange
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
        var found = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([
                    new(newTag1.Id, TagSearchScope.Excluded, null),
                    new(newTag2.Id, TagSearchScope.Excluded, null),
                ]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        // assert
        found.Should().Contain(file4Id);
        found.Should().NotContainAll([file1Id, file2Id, file3Id, file5Id]);
    }

    [Fact]
    public async Task SearchFilesCountFast()
    {
        // arrange
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
        var found1GirlCount = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast/count",
                new SearchFilesFastQuery([new(newTag1.Id, TagSearchScope.Included, null)]))
            .ReadResult<int>();
        
        var foundSoloCount = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast/count",
                new SearchFilesFastQuery([new(newTag3.Id, TagSearchScope.Included, null)]))
            .ReadResult<int>();
        
        var foundBlueHairCount = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast/count",
                new SearchFilesFastQuery([new(newTag4.Id, TagSearchScope.Included, null)]))
            .ReadResult<int>();
        
        var found2GirlCount = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast/count",
                new SearchFilesFastQuery([new(newTag2.Id, TagSearchScope.Included, null)]))
            .ReadResult<int>();

        var found1GirlBlueHairCount = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast/count",
                new SearchFilesFastQuery([
                    new(newTag4.Id, TagSearchScope.Included, null),
                    new(newTag1.Id, TagSearchScope.Included, null)
                ]))
            .ReadResult<int>();

        var found1GirlExcludeBlueHairCount = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast/count",
                new SearchFilesFastQuery([
                    new(newTag4.Id, TagSearchScope.Excluded, null),
                    new(newTag1.Id, TagSearchScope.Included, null)
                ]))
            .ReadResult<int>();
        
        // assert
        found1GirlCount.Should().Be(2);
        foundSoloCount.Should().Be(3);
        foundBlueHairCount.Should().Be(3);
        found1GirlBlueHairCount.Should().Be(1);
        found2GirlCount.Should().Be(2);
        found1GirlExcludeBlueHairCount.Should().Be(1);
    }
    
    
    [Fact]
    public async Task SearchFilesWithValuesFast()
    {
        // arrange
        var httpClient = _webApp.Client;

        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();
        var file3Id = Guid.NewGuid();
        var file4Id = Guid.NewGuid();
        var file5Id = Guid.NewGuid();

        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var newTag1 = await CreateNewTag(httpClient, types, "feeling", true);
        var newTag2 = await CreateNewTag(httpClient, types, "rate", true);
        var newTag3 = await CreateNewTag(httpClient, types, "score", true);

        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
        [
            new(file1Id, MetadataSource.Manual, newTag1.Id, "feeling good"),
            new(file1Id, MetadataSource.Manual, newTag3.Id, "not scored"),

            new(file2Id, MetadataSource.Manual, newTag2.Id, "5"),
            
            new(file3Id, MetadataSource.Manual, newTag1.Id, "feeling bad"),
            new(file3Id, MetadataSource.Manual, newTag3.Id, "scored"),
            
            new(file4Id, MetadataSource.Manual, newTag1.Id, "not feeling"),
            new(file4Id, MetadataSource.Manual, newTag3.Id, "a little bit scored"),

            new(file5Id, MetadataSource.Manual, newTag2.Id, "3"),
        ], SameTagHandleStrategy.ReplaceExistingValue));
        
        
        // act
        var foundFeeling = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag1.Id, TagSearchScope.Included, "=feeling*")]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        var foundNotFeeling1 = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag1.Id, TagSearchScope.Included, "!=feeling*")]))
            .ReadResult<IReadOnlyCollection<Guid>>();

        var foundNotFeeling2 = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag1.Id, TagSearchScope.Included, "!feeling*")]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        var foundNotFeeling3 = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag1.Id, TagSearchScope.Excluded, "feeling*")]))
            .ReadResult<IReadOnlyCollection<Guid>>();

        var foundNotFeeling4 = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag1.Id, TagSearchScope.Excluded, "feeling*")]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        var foundNotFeeling5 = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag1.Id, TagSearchScope.Included, "!=feeling good")]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        var foundScored1 = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag3.Id, TagSearchScope.Included, "*scored")]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        var foundScored2 = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag3.Id, TagSearchScope.Included, "*scor*")]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        var notFoundScored1 = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag3.Id, TagSearchScope.Included, "*scor")]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        var foundScored3 = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag3.Id, TagSearchScope.Included, "!*scor*")]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        var foundScored4 = await httpClient
            .PostAsJsonAsync(
                "/files/search-fast",
                new SearchFilesFastQuery([new(newTag3.Id, TagSearchScope.Included, "!*scor")]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        // assert
        foundFeeling.Should().HaveCount(2);
        foundFeeling.Should().BeEquivalentTo([file1Id, file3Id]);
        
        foundNotFeeling1.Should().HaveCount(1);
        foundNotFeeling1.Should().BeEquivalentTo([file4Id]);
        
        foundNotFeeling2.Should().HaveCount(1);
        foundNotFeeling2.Should().BeEquivalentTo([file4Id]);
        
        foundNotFeeling3.Should().HaveCountGreaterThanOrEqualTo(3);
        foundNotFeeling3.Should().ContainAll([file4Id, file2Id, file5Id]);
        foundNotFeeling3.Should().NotContainAll([file3Id, file1Id]);
        
        foundNotFeeling4.Should().HaveCountGreaterThanOrEqualTo(3);
        foundNotFeeling4.Should().ContainAll([file4Id, file2Id, file5Id]);
        foundNotFeeling4.Should().NotContainAll([file3Id, file1Id]);
        
        foundScored1.Should().HaveCount(3);
        foundScored1.Should().BeEquivalentTo([file1Id, file3Id, file4Id]);
        
        foundScored2.Should().HaveCount(3);
        foundScored2.Should().BeEquivalentTo([file1Id, file3Id, file4Id]);
        
        notFoundScored1.Should().HaveCount(0);

        foundScored3.Should().HaveCount(0);

        foundScored4.Should().HaveCount(3);
        foundScored4.Should().BeEquivalentTo([file1Id, file3Id, file4Id]);
        
        foundNotFeeling5.Should().HaveCount(2);
        foundNotFeeling5.Should().BeEquivalentTo([file3Id, file4Id]);
    }
    
    [Fact]
    public async Task FilterFiles()
    {
        // arrange
        var httpClient = _webApp.Client;

        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();
        var file3Id = Guid.NewGuid();
        var file4Id = Guid.NewGuid();
        var file5Id = Guid.NewGuid();
        
        var file6Id = Guid.NewGuid();
        var file7Id = Guid.NewGuid();
        var file8Id = Guid.NewGuid();
        var file9Id = Guid.NewGuid();
        var file10Id = Guid.NewGuid();

        var allFiles = new[]
            { file1Id, file2Id, file3Id, file4Id, file5Id, file6Id, file7Id, file8Id, file9Id, file10Id }; 

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
        var filtered1Girl = await httpClient
            .PostAsJsonAsync(
                "/files/filter",
                new FilterFilesQuery(allFiles, [new(newTag1.Id, TagSearchScope.Included, null)]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        var filteredSolo = await httpClient
            .PostAsJsonAsync(
                "/files/filter",
                new FilterFilesQuery(allFiles, [new(newTag3.Id, TagSearchScope.Included, null)]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        var filteredBlueHair = await httpClient
            .PostAsJsonAsync(
                "/files/filter",
                new FilterFilesQuery(allFiles, [new(newTag4.Id, TagSearchScope.Included, null)]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        var filtered2Girl = await httpClient
            .PostAsJsonAsync(
                "/files/filter",
                new FilterFilesQuery(allFiles, [new(newTag2.Id, TagSearchScope.Included, null)]))
            .ReadResult<IReadOnlyCollection<Guid>>();

        var filtered1GirlBlueHair = await httpClient
            .PostAsJsonAsync(
                "/files/filter",
                new FilterFilesQuery(allFiles, [
                    new(newTag4.Id, TagSearchScope.Included, null),
                    new(newTag1.Id, TagSearchScope.Included, null)
                ]))
            .ReadResult<IReadOnlyCollection<Guid>>();

        var filtered1GirlExcludeBlueHair = await httpClient
            .PostAsJsonAsync(
                "/files/filter",
                new FilterFilesQuery(allFiles, [
                    new(newTag4.Id, TagSearchScope.Excluded, null),
                    new(newTag1.Id, TagSearchScope.Included, null)
                ]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        // assert
        filtered1Girl.Should().HaveCount(2);
        filtered1Girl.Should().BeEquivalentTo([file1Id, file3Id]);
        filteredSolo.Should().HaveCount(3);
        filteredSolo.Should().BeEquivalentTo([file1Id, file3Id, file4Id]);
        filteredBlueHair.Should().HaveCount(3);
        filteredBlueHair.Should().BeEquivalentTo([file1Id, file2Id, file4Id]);
        filtered1GirlBlueHair.Should().HaveCount(1);
        filtered1GirlBlueHair.Should().BeEquivalentTo([file1Id]);
        filtered2Girl.Should().HaveCount(2);
        filtered2Girl.Should().BeEquivalentTo([file2Id, file5Id]);
        filtered1GirlExcludeBlueHair.Should().HaveCount(1);
        filtered1GirlExcludeBlueHair.Should().BeEquivalentTo([file3Id]);
    }

    [Fact]
    public async Task FilterFilesOnlyExclude()
    {
        // arrange
        var httpClient = _webApp.Client;

        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();
        var file3Id = Guid.NewGuid();
        var file4Id = Guid.NewGuid();
        var file5Id = Guid.NewGuid();
        
        var file6Id = Guid.NewGuid();
        var file7Id = Guid.NewGuid();
        var file8Id = Guid.NewGuid();
        var file9Id = Guid.NewGuid();
        var file10Id = Guid.NewGuid();

        var allFiles = new[]
            { file1Id, file2Id, file3Id, file4Id, file5Id, file6Id, file7Id, file8Id, file9Id, file10Id }; 

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
        var filtered = await httpClient
            .PostAsJsonAsync(
                "/files/filter",
                new FilterFilesQuery(allFiles, [
                    new(newTag1.Id, TagSearchScope.Excluded, null),
                    new(newTag2.Id, TagSearchScope.Excluded, null),
                ]))
            .ReadResult<IReadOnlyCollection<Guid>>();
        
        // assert
        filtered.Should().HaveCount(6);
        filtered.Should().BeEquivalentTo([file4Id, file6Id, file7Id, file8Id, file9Id, file10Id]);
    }

    [Fact]
    public async Task FilterFilesCount()
    {
        // arrange
        var httpClient = _webApp.Client;

        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();
        var file3Id = Guid.NewGuid();
        var file4Id = Guid.NewGuid();
        var file5Id = Guid.NewGuid();
        
        var file6Id = Guid.NewGuid();
        var file7Id = Guid.NewGuid();
        var file8Id = Guid.NewGuid();
        var file9Id = Guid.NewGuid();
        var file10Id = Guid.NewGuid();

        var allFiles = new[]
            { file1Id, file2Id, file3Id, file4Id, file5Id, file6Id, file7Id, file8Id, file9Id, file10Id }; 

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
        var filtered1GirlCount = await httpClient
            .PostAsJsonAsync(
                "/files/filter/count",
                new FilterFilesQuery(allFiles, [new(newTag1.Id, TagSearchScope.Included, null)]))
            .ReadResult<int>();
        
        var filteredSoloCount = await httpClient
            .PostAsJsonAsync(
                "/files/filter/count",
                new FilterFilesQuery(allFiles, [new(newTag3.Id, TagSearchScope.Included, null)]))
            .ReadResult<int>();
        
        var filteredBlueHairCount = await httpClient
            .PostAsJsonAsync(
                "/files/filter/count",
                new FilterFilesQuery(allFiles, [new(newTag4.Id, TagSearchScope.Included, null)]))
            .ReadResult<int>();
        
        var filtered2GirlCount = await httpClient
            .PostAsJsonAsync(
                "/files/filter/count",
                new FilterFilesQuery(allFiles, [new(newTag2.Id, TagSearchScope.Included, null)]))
            .ReadResult<int>();

        var filtered1GirlBlueHairCount = await httpClient
            .PostAsJsonAsync(
                "/files/filter/count",
                new FilterFilesQuery(allFiles, [
                    new(newTag4.Id, TagSearchScope.Included, null),
                    new(newTag1.Id, TagSearchScope.Included, null)
                ]))
            .ReadResult<int>();

        var filtered1GirlExcludeBlueHairCount = await httpClient
            .PostAsJsonAsync(
                "/files/filter/count",
                new FilterFilesQuery(allFiles, [
                    new(newTag4.Id, TagSearchScope.Excluded, null),
                    new(newTag1.Id, TagSearchScope.Included, null)
                ]))
            .ReadResult<int>();
        
        // assert
        filtered1GirlCount.Should().Be(2);
        filteredSoloCount.Should().Be(3);
        filteredBlueHairCount.Should().Be(3);
        filtered1GirlBlueHairCount.Should().Be(1);
        filtered2GirlCount.Should().Be(2);
        filtered1GirlExcludeBlueHairCount.Should().Be(1);
    }

    [Fact]
    public async Task FilterFilesCountOnlyExclude()
    {
        // arrange
        var httpClient = _webApp.Client;

        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();
        var file3Id = Guid.NewGuid();
        var file4Id = Guid.NewGuid();
        var file5Id = Guid.NewGuid();
        
        var file6Id = Guid.NewGuid();
        var file7Id = Guid.NewGuid();
        var file8Id = Guid.NewGuid();
        var file9Id = Guid.NewGuid();
        var file10Id = Guid.NewGuid();

        var allFiles = new[]
            { file1Id, file2Id, file3Id, file4Id, file5Id, file6Id, file7Id, file8Id, file9Id, file10Id }; 

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
        var filtered = await httpClient
            .PostAsJsonAsync(
                "/files/filter/count",
                new FilterFilesQuery(allFiles, [
                    new(newTag1.Id, TagSearchScope.Excluded, null),
                    new(newTag2.Id, TagSearchScope.Excluded, null),
                ]))
            .ReadResult<int>();
        
        // assert
        filtered.Should().Be(6);
    }

    [Fact]
    public async Task BindTagsShouldReplaceExistingValues()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
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
        using var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
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
        using var scope = _webApp.GetScope();
        var context = _webApp.GetDbContext(scope);
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

    [Fact]
    public async Task GetFileInfo()
    {
        // arrange
        using var scope = _webApp.GetScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var httpClient = _webApp.Client;

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
                    SourceId: "1",
                    Label: "Bounce sound translation" + Guid.NewGuid(),
                    PositionFromLeft: 10,
                    PositionFromTop: 20,
                    Width: 30,
                    Height: 40)
            ]);

        await mediator.Send(command);
        
        // act
        var fileInfo = await httpClient.GetFromJsonAsync<DetailedFileInfo>($"/files/{command.FileId}");

        // assert
        var tags = fileInfo!.Tags!;
        var notes = fileInfo.Notes!;

        fileInfo.Should().NotBeNull();
        tags.Should().HaveCount(4);
        tags.Should().Contain(x => x.Tag!.Name == newTags[0].Name);
        tags.Should().Contain(x => x.Tag!.Name == newTags[1].Name);
        tags.Should().Contain(x => x.Tag!.Name == newTags[2].Name);
        tags.Should().Contain(x => x.Tag!.Name == newTags[3].Name);
        tags.First(x => x.Tag!.Name == newTags[3].Name).Value.Should().Be("Questionable");
        tags.First(x => x.Tag!.Name == newTags[3].Name).Tag!.Synonyms.Should().BeEquivalentTo(newTags[3].Synonyms);
        notes.Should().HaveCount(1);
        notes.First().Label.Should().Be(command.Notes.First().Label);
        notes.First().PositionFromLeft.Should().Be(command.Notes.First().PositionFromLeft);
        notes.First().PositionFromTop.Should().Be(command.Notes.First().PositionFromTop);
        notes.First().Width.Should().Be(command.Notes.First().Width);
        notes.First().Height.Should().Be(command.Notes.First().Height);
    }

    [Fact]
    public async Task GetRelativesInfo()
    {
        // arrange
        var httpClient = _webApp.Client;

        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();
        var file3Id = Guid.NewGuid();
        var file4Id = Guid.NewGuid();
        var file5Id = Guid.NewGuid();

        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var parentTag = await CreateNewTag(httpClient, types, "ParentMd5", true, true);
        var childTag = await CreateNewTag(httpClient, types, "Child", true, true);
        var newTag3 = await CreateNewTag(httpClient, types, "solo");
        var newTag4 = await CreateNewTag(httpClient, types, "1girl");

        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
        [
            new(file1Id, MetadataSource.Manual, parentTag.Id, ImageHash4),
            new(file1Id, MetadataSource.Manual, newTag3.Id, null),
            new(file1Id, MetadataSource.Manual, newTag4.Id, null),

            new(file2Id, MetadataSource.Manual, parentTag.Id, ImageHash4),
            new(file2Id, MetadataSource.Manual, newTag4.Id, null),
            
            new(file3Id, MetadataSource.Manual, childTag.Id, $"4066000:{ImageHash4}"),
            new(file3Id, MetadataSource.Manual, childTag.Id, $"4066001:{ImageHash5}"),
            new(file3Id, MetadataSource.Manual, newTag3.Id, null),
            
            new(file4Id, MetadataSource.Manual, newTag3.Id, null),
            new(file4Id, MetadataSource.Manual, newTag4.Id, null),

            new(file5Id, MetadataSource.Manual, parentTag.Id, ImageHash5),
        ], SameTagHandleStrategy.AddNewFileTag));
        
        
        // act
        var relativeInfo1 = await httpClient
            .GetFromJsonAsync<IReadOnlyCollection<RelativeInfo>>($"/files/{ImageHash4}/relatives");

        var relativeInfo2 = await httpClient
            .GetFromJsonAsync<IReadOnlyCollection<RelativeInfo>>($"/files/{ImageHash5}/relatives");
        
        // assert
        relativeInfo1.Should().HaveCount(3);
        relativeInfo1.Should().Contain(x => x.FileInfo!.FileId == file1Id && x.RelativesType == RelativeType.Parent);
        relativeInfo1.Should().Contain(x => x.FileInfo!.FileId == file2Id && x.RelativesType == RelativeType.Parent);
        relativeInfo1.Should().Contain(x => x.FileInfo!.FileId == file3Id && x.RelativesType == RelativeType.Child);
        
        relativeInfo2.Should().HaveCount(2);
        relativeInfo2.Should().Contain(x => x.FileInfo!.FileId == file3Id && x.RelativesType == RelativeType.Child);
        relativeInfo2.Should().Contain(x => x.FileInfo!.FileId == file5Id && x.RelativesType == RelativeType.Parent);
    }

    [Fact]
    public async Task GetRelativesInfoBatch()
    {
        // arrange
        var httpClient = _webApp.Client;

        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();
        var file3Id = Guid.NewGuid();
        var file4Id = Guid.NewGuid();
        var file5Id = Guid.NewGuid();

        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var parentTag = await CreateNewTag(httpClient, types, "ParentMd5", true, true);
        var childTag = await CreateNewTag(httpClient, types, "Child", true, true);
        var newTag3 = await CreateNewTag(httpClient, types, "solo");
        var newTag4 = await CreateNewTag(httpClient, types, "1girl");

        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
        [
            new(file1Id, MetadataSource.Manual, parentTag.Id, ImageHash1),
            new(file1Id, MetadataSource.Manual, newTag3.Id, null),
            new(file1Id, MetadataSource.Manual, newTag4.Id, null),

            new(file2Id, MetadataSource.Manual, parentTag.Id, ImageHash1),
            new(file2Id, MetadataSource.Manual, newTag4.Id, null),
            
            new(file3Id, MetadataSource.Manual, childTag.Id, $"4066000:{ImageHash1}"),
            new(file3Id, MetadataSource.Manual, childTag.Id, $"4066001:{ImageHash2}"),
            new(file3Id, MetadataSource.Manual, newTag3.Id, null),
            
            new(file4Id, MetadataSource.Manual, newTag3.Id, null),
            new(file4Id, MetadataSource.Manual, newTag4.Id, null),

            new(file5Id, MetadataSource.Manual, parentTag.Id, ImageHash3),
        ], SameTagHandleStrategy.AddNewFileTag));
        
        
        // act
        var relativeInfoBatch = await httpClient
            .PostAsJsonAsync("/files/relatives", new[] { ImageHash1, ImageHash2, ImageHash6 })
            .ReadResult<IReadOnlyCollection<RelativeShortInfo>>();
        
        // assert
        relativeInfoBatch.Should().HaveCount(3);
        relativeInfoBatch.Should().Contain(x => x.Hash == ImageHash1 && x.RelativeType == RelativeType.Parent);
        relativeInfoBatch.Should().Contain(x => x.Hash == ImageHash2 && x.RelativeType == RelativeType.Child);
        relativeInfoBatch.Should().Contain(x => x.Hash == ImageHash6 && x.RelativeType == null);
    }

    [Fact]
    public async Task GetRelativesInfoBatchShouldSaveToCache()
    {
        // arrange
        var httpClient = _webApp.Client;
        var cache = _webApp.GetScope().ServiceProvider.GetRequiredService<IMemoryCache>();

        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();
        var file3Id = Guid.NewGuid();
        var file4Id = Guid.NewGuid();
        var file5Id = Guid.NewGuid();

        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var parentTag = await CreateNewTag(httpClient, types, "ParentMd5", true, true);
        var childTag = await CreateNewTag(httpClient, types, "Child", true, true);
        var newTag3 = await CreateNewTag(httpClient, types, "solo");
        var newTag4 = await CreateNewTag(httpClient, types, "1girl");

        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
        [
            new(file1Id, MetadataSource.Manual, parentTag.Id, ImageHash1),
            new(file1Id, MetadataSource.Manual, newTag3.Id, null),
            new(file1Id, MetadataSource.Manual, newTag4.Id, null),

            new(file2Id, MetadataSource.Manual, parentTag.Id, ImageHash1),
            new(file2Id, MetadataSource.Manual, newTag4.Id, null),
            
            new(file3Id, MetadataSource.Manual, childTag.Id, $"4066000:{ImageHash1}"),
            new(file3Id, MetadataSource.Manual, childTag.Id, $"4066001:{ImageHash2}"),
            new(file3Id, MetadataSource.Manual, newTag3.Id, null),
            
            new(file4Id, MetadataSource.Manual, newTag3.Id, null),
            new(file4Id, MetadataSource.Manual, newTag4.Id, null),

            new(file5Id, MetadataSource.Manual, parentTag.Id, ImageHash3),
        ], SameTagHandleStrategy.AddNewFileTag));
        
        // act
        await httpClient
            .PostAsJsonAsync("/files/relatives", new[] { ImageHash1, ImageHash2 })
            .ReadResult<IReadOnlyCollection<RelativeShortInfo>>();
        
        // assert
        var cachedValue =
            cache.Get<AppRelativeShortInfo>(
                $"FilterHashes_{ImageHash1}");
        cachedValue.Should().NotBeNull();
        cachedValue!.Hash.Should().Be(ImageHash1);
        cachedValue.RelativeType.Should().Be(AppRelativeType.Parent);

        cachedValue =
            cache.Get<AppRelativeShortInfo>(
                $"FilterHashes_{ImageHash2}");
        cachedValue.Should().NotBeNull();
        cachedValue!.Hash.Should().Be(ImageHash2);
        cachedValue.RelativeType.Should().Be(AppRelativeType.Child);
    }

    [Fact]
    public async Task GetRelativesInfoBatchShouldUseCacheForNextCalls()
    {
        // arrange
        var httpClient = _webApp.Client;
        var cache = _webApp.GetScope().ServiceProvider.GetRequiredService<IMemoryCache>();

        var file1Id = Guid.NewGuid();
        var file2Id = Guid.NewGuid();
        var file3Id = Guid.NewGuid();
        var file4Id = Guid.NewGuid();
        var file5Id = Guid.NewGuid();

        var types = await httpClient.GetFromJsonAsync<IReadOnlyCollection<TagType>>("/tags/types");
        var parentTag = await CreateNewTag(httpClient, types, "ParentMd5", true, true);
        var childTag = await CreateNewTag(httpClient, types, "Child", true, true);
        var newTag3 = await CreateNewTag(httpClient, types, "solo");
        var newTag4 = await CreateNewTag(httpClient, types, "1girl");

        await httpClient.PostAsJsonAsync("/files/tags", new BindTagsCommand(
        [
            new(file1Id, MetadataSource.Manual, parentTag.Id, ImageHash1),
            new(file1Id, MetadataSource.Manual, newTag3.Id, null),
            new(file1Id, MetadataSource.Manual, newTag4.Id, null),

            new(file2Id, MetadataSource.Manual, parentTag.Id, ImageHash1),
            new(file2Id, MetadataSource.Manual, newTag4.Id, null),
            
            new(file3Id, MetadataSource.Manual, childTag.Id, $"4066000:{ImageHash1}"),
            new(file3Id, MetadataSource.Manual, childTag.Id, $"4066001:{ImageHash2}"),
            new(file3Id, MetadataSource.Manual, newTag3.Id, null),
            
            new(file4Id, MetadataSource.Manual, newTag3.Id, null),
            new(file4Id, MetadataSource.Manual, newTag4.Id, null),

            new(file5Id, MetadataSource.Manual, parentTag.Id, ImageHash3),
        ], SameTagHandleStrategy.AddNewFileTag));
        
        await httpClient
            .PostAsJsonAsync("/files/relatives", new[] { ImageHash1, ImageHash2 })
            .ReadResult<IReadOnlyCollection<RelativeShortInfo>>();

        // override cached values to checked that http method actually returns them and not db values
        cache.Set($"FilterHashes_{ImageHash1}",
            new AppRelativeShortInfo(
                ImageHash1, 
                AppRelativeType.Child));
        
        cache.Set($"FilterHashes_{ImageHash2}",
            new AppRelativeShortInfo(ImageHash2, AppRelativeType.Parent));
        
        // act
        var relativeInfoBatch = await httpClient
            .PostAsJsonAsync("/files/relatives", new[] { ImageHash1, ImageHash2 })
            .ReadResult<IReadOnlyCollection<RelativeShortInfo>>();
        
        // assert
        relativeInfoBatch.Should().HaveCount(2);
        relativeInfoBatch.Should().Contain(x => x.Hash == ImageHash1 && x.RelativeType == RelativeType.Child);
        relativeInfoBatch.Should().Contain(x => x.Hash == ImageHash2 && x.RelativeType == RelativeType.Parent);
    }

    [Fact]
    public async Task GetRelativesInfoBatchWithEmptyCollection()
    {
        // arrange
        var httpClient = _webApp.Client;
        
        // act
        var relativeInfoBatch = await httpClient
            .PostAsJsonAsync("/files/relatives", Array.Empty<string>())
            .ReadResult<IReadOnlyCollection<RelativeShortInfo>>();
        
        // assert
        relativeInfoBatch.Should().NotBeNull();
        relativeInfoBatch.Should().HaveCount(0);
    }
    
    private static async Task<Tag> CreateNewTag(
        HttpClient client,
        IReadOnlyCollection<TagType>? types,
        string namePrefix,
        bool hasValue = false,
        bool disableRandomness = false)
    {
        var typeId = types!.First(x => x.Name == "General").Id;
        return await client
            .PostAsJsonAsync(
                "/tags",
                new CreateTagCommand(
                    typeId,
                    namePrefix + (disableRandomness ? "" : Guid.NewGuid()),
                    hasValue,
                    [],
                    Domain.TagAggregate.TagOptions.None))
            .ReadResult<Tag>();
    }
}
