using System.Reactive.Linq;
using AwesomeAssertions;
using ImoutoRebirth.Room.UI.FileSystemEvents;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ImoutoRebirth.Room.IntegrationTests;

[CollectionDefinition("FileSystemWatcherEventStreamPreparation")]
public class FileSystemWatcherEventStreamCollection : ICollectionFixture<FileSystemWatcherEventStreamTestService>;

public class FileSystemWatcherEventStreamTestService : IDisposable
{
    private readonly List<DirectoryInfo> _createdDirectories = new(); 
    
    public DirectoryInfo CreateTempDirectory()
    {
        var di = new DirectoryInfo(Path.Combine("./file-system-tests", Guid.NewGuid().ToString()));
        di.Create();

        _createdDirectories.Add(di);
        return di;
    }
    
    public void Dispose()
    {
        foreach (var createdDirectory in _createdDirectories) 
            createdDirectory.Delete(true);
    }
}

[Collection("FileSystemWatcherEventStreamPreparation")]
public class FileSystemWatcherEventStreamTests
{
    private readonly FileSystemWatcherEventStreamTestService _testService;

    public FileSystemWatcherEventStreamTests(FileSystemWatcherEventStreamTestService testService) 
        => _testService = testService;

    [Fact]
    public async Task FileSystemWatcherEventStreamProduceEvents()
    {
        // arrange
        var directory = _testService.CreateTempDirectory();
        
        var stream = new FileSystemWatcherEventStream(
            [directory.FullName],
            NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite, 
            true,
            CancellationToken.None,
            NullLogger.Instance);

        var task = new TaskCompletionSource<bool>();
        await stream.Observable.SubscribeAsync(x =>
        {
            if (!task.Task.IsCompleted)
                task.SetResult(true);
        });

        // act
        await File.WriteAllTextAsync(Path.Combine(directory.FullName, "test.txt"), "test");
        
        // assert
        var delayTask = Task.Delay(5000);
        await Task.WhenAny(task.Task, delayTask);
        
        Assert.True(task.Task.IsCompleted);
    }

    [Fact]
    public async Task FileSystemWatcherEventStreamThrottleShouldCallSubscribeOn4Files()
    {
        // arrange
        var directory = _testService.CreateTempDirectory();
        
        var stream = new FileSystemWatcherEventStream(
            [directory.FullName],
            NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite, 
            true,
            CancellationToken.None,
            NullLogger.Instance);

        var counterCalls = 0;
        var filesCount = 0;
        await stream.Observable
            .Throttle(TimeSpan.FromMilliseconds(250))
            .SubscribeAsync(x =>
            {
                counterCalls++;
                filesCount = directory.GetFiles().Length;
            });

        // act
        await File.WriteAllTextAsync(Path.Combine(directory.FullName, "test1.txt"), "test1");
        await File.WriteAllTextAsync(Path.Combine(directory.FullName, "test2.txt"), "test2");
        await File.WriteAllTextAsync(Path.Combine(directory.FullName, "test3.txt"), "test3");
        await File.WriteAllTextAsync(Path.Combine(directory.FullName, "test4.txt"), "test4");
        
        // assert
        await Task.Delay(5000);

        counterCalls.Should().Be(1);
        filesCount.Should().Be(4);
    }

    [Fact]
    public async Task FileSystemWatcherEventStreamThrottleShouldCallSubscribeOn4FilesAndOn1Files()
    {
        // arrange
        var directory = _testService.CreateTempDirectory();
        
        var stream = new FileSystemWatcherEventStream(
            [directory.FullName],
            NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite, 
            true,
            CancellationToken.None,
            NullLogger.Instance);

        var counterCalls = 0;
        var filesCount = 0;
        await stream.Observable
            .Throttle(TimeSpan.FromMilliseconds(250))
            .SubscribeAsync(x =>
            {
                counterCalls++;
                filesCount = directory.GetFiles().Length;
            });

        // act
        await File.WriteAllTextAsync(Path.Combine(directory.FullName, "test1.txt"), "test1");
        await File.WriteAllTextAsync(Path.Combine(directory.FullName, "test2.txt"), "test2");
        await File.WriteAllTextAsync(Path.Combine(directory.FullName, "test3.txt"), "test3");
        await File.WriteAllTextAsync(Path.Combine(directory.FullName, "test4.txt"), "test4");
        await Task.Delay(1000);
        await File.WriteAllTextAsync(Path.Combine(directory.FullName, "test5.txt"), "test5");
        
        // assert
        await Task.Delay(5000);

        counterCalls.Should().Be(2);
        filesCount.Should().Be(5);
    }
}
