using AwesomeAssertions;
using ImoutoRebirth.Room.Domain;
using Xunit;

namespace ImoutoRebirth.Room.IntegrationTests;

public class CollectionFileTests
{
    [Fact]
    public void CollectionFile_Rename_WorksAsExpected()
    {
        // arrange
        var collectionFile = new CollectionFile(
            Guid.NewGuid(),
            Guid.NewGuid(),
            @"C:\some\path\file name.jpg",
            "md5hash",
            1234,
            @"C:\original\path\file name.jpg");
        
        // act
        collectionFile.Rename("new file name.newjpg");

        // assert
        collectionFile.Path
            .Should().Be("C:\\some\\path\\new file name.newjpg");
    }
}
