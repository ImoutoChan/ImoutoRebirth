using System.Reflection;
using Imouto.BooruParser.Loaders;
using Imouto.BooruParser.Model.Base;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;

internal class ExtendedYandereLoader : IBooruAsyncLoader
{
    private static readonly MethodInfo ParentSetter;

    static ExtendedYandereLoader() 
        => ParentSetter = typeof(Post).GetProperties().First(x => x.Name == nameof(Post.ParentId)).SetMethod!;

    private readonly IBooruAsyncLoader _yandereLoader;

    public ExtendedYandereLoader(IBooruAsyncLoader yandereLoader) => _yandereLoader = yandereLoader;

    public async Task<Post> LoadPostAsync(int postId)
    {
        var mainPost = await _yandereLoader.LoadPostAsync(postId);

        var childrenIds = mainPost.ChildrenIds.Select(x => x.Split(':').First()).ToList();
        var childrenLoadTasks = childrenIds.Select(int.Parse).Select(x => _yandereLoader.LoadPostAsync(x)).ToList();

        await Task.WhenAll(childrenLoadTasks);

        var childrenIdsWithMd5 = childrenLoadTasks.Select(x => x.Result).Select(x => $"{x.PostId}:{x.Md5}");
        
        mainPost.ChildrenIds.Clear();
        mainPost.ChildrenIds.AddRange(childrenIdsWithMd5);

        if (mainPost.ParentId == null) 
            return mainPost;
        
        var parentId = mainPost.ParentId.Split(':').First();
        
        var parentPost = await _yandereLoader.LoadPostAsync(int.Parse(parentId));
        var parentIdWithMd5 = $"{parentPost.PostId}:{parentPost.Md5}";

        ParentSetter.Invoke(mainPost, new[] { parentIdWithMd5 });

        return mainPost;
    }

    public async Task<SearchResult> LoadSearchResultAsync(string tagsString) 
        => await _yandereLoader.LoadSearchResultAsync(tagsString);

    public async Task<List<NoteUpdateEntry>> LoadNotesHistoryAsync(DateTime lastUpdateTime) 
        => await _yandereLoader.LoadNotesHistoryAsync(lastUpdateTime);

    public async Task<List<PostUpdateEntry>> LoadTagHistoryUpToAsync(DateTime toDate) 
        => await _yandereLoader.LoadTagHistoryUpToAsync(toDate);

    public async Task<List<PostUpdateEntry>> LoadTagHistoryFromAsync(int fromId) 
        => await _yandereLoader.LoadTagHistoryFromAsync(fromId);

    public async Task<List<PostUpdateEntry>> LoadFirstTagHistoryPageAsync() 
        => await _yandereLoader.LoadFirstTagHistoryPageAsync();

    public async Task<SearchResult> LoadPopularAsync(PopularType type) 
        => await _yandereLoader.LoadPopularAsync(type);
}
