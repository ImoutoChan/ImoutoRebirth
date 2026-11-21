using ImoutoRebirth.Lilin.DataAccess.Entities;
using ImoutoRebirth.Lilin.Infrastructure;

namespace ImoutoRebirth.Lilin.IntegrationTests;

public class TagValueFilterTests
{
    [Theory]
    [InlineData("5", "5", true)]
    [InlineData("5", "55", false)]
    [InlineData("abc*", "abcdef", true)]
    [InlineData("abc*", "abx", false)]
    [InlineData("*abc*", "xabcy", true)]
    [InlineData("*abc*", "xabx", false)]
    [InlineData("!=*abc*", "xabcy", false)]
    [InlineData("!=*abc*", "xabx", true)]
    [InlineData("*", null, false)]
    [InlineData("*", "any", true)]
    [InlineData("=", "", true)]
    public void MatchesTagValue(string filter, string? value, bool expected)
    {
        var tags = new[] { new FileTagEntity { Value = value } }.AsQueryable();

        Assert.Equal(expected, tags.MakeValueFilter(filter).Any());
    }
}
