using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using Serilog;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.Utils;

namespace ImoutoRebirth.Navigator.Services.ImoutoViewer;

internal class ImoutoViewerService : IImoutoViewerService
{
    public async Task OpenFile(string path, Guid? collectionId, IEnumerable<SearchTag> searchTags, IEnumerable<string> playlist)
    {
        var playlistId = Guid.NewGuid();
        NamedPipeServerStream? pipe = null;

        try
        {
            var executable = Associations.AssocQueryString(Associations.AssocStr.ASSOCSTR_EXECUTABLE, Path.GetExtension(path));
            if (string.Equals(Path.GetFileNameWithoutExtension(executable), "ImoutoViewer", StringComparison.OrdinalIgnoreCase))
            {
                pipe = new NamedPipeServerStream(
                    $"ImoutoViewer-{playlistId:N}",
                    PipeDirection.Out,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            }

            var args = new ImoutoViewerArgs(collectionId, GetDto(searchTags).ToArray()) { PlaylistId = pipe == null ? null : playlistId };
            var start = new ProcessStartInfo(executable);
            start.ArgumentList.Add(path);
            start.ArgumentList.Add($"-nav-search={Base64Serializer.Serialize(args)}");

            using var process = Process.Start(start);
        }
        catch (Exception e) when (e is InvalidOperationException or System.ComponentModel.Win32Exception or IOException)
        {
            pipe?.Dispose();
            using var process = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            return;
        }

        if (pipe == null)
            return;

        using (pipe)
        using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
        {
            var paths = playlist.ToArray();

            try
            {
                await pipe.WaitForConnectionAsync(timeout.Token).ConfigureAwait(false);
                await JsonSerializer.SerializeAsync(pipe, paths, cancellationToken: timeout.Token).ConfigureAwait(false);
            }
            catch (Exception e) when (e is IOException or OperationCanceledException)
            {
                Log.Debug(e, "Viewer did not receive the Navigator playlist");
            }
        }
    }

    private static IEnumerable<SearchTagDto> GetDto(IEnumerable<SearchTag> searchTags) 
        => searchTags.Select(x => new SearchTagDto(x.Tag.Id, x.Value, x.SearchType));
}
