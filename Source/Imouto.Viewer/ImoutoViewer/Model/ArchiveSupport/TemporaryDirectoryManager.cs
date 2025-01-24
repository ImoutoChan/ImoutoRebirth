using System.IO;

namespace ImoutoViewer.Model.ArchiveSupport;

internal class TemporaryDirectoryManager : IDisposable
{
    private readonly string _tempDir;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly string _lockFilePath;
    private FileStream? _lockFileStream;

    public TemporaryDirectoryManager()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ArchiveImageList", Guid.NewGuid().ToString());
        _lockFilePath = Path.Combine(_tempDir, "lock");

        Directory.CreateDirectory(_tempDir);

        _lockFileStream = new FileStream(_lockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
    }

    public string TempDirectoryPath => _tempDir;

    public static void CleanupOldTempDirectories()
    {
        var baseTempPath = Path.Combine(Path.GetTempPath(), "ArchiveImageList");
        if (!Directory.Exists(baseTempPath))
            return;

        foreach (var dir in Directory.GetDirectories(baseTempPath))
        {
            var lockFile = Path.Combine(dir, "lock");
            try
            {
                if (File.Exists(lockFile))
                {
                    using var stream = new FileStream(lockFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    stream.Close();
                }

                Directory.Delete(dir, true);
            }
            catch (IOException)
            {
                // Directory is locked, skip it
                Console.WriteLine($"Directory {dir} is in use, skipping.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up directory {dir}: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        try
        {
            _lockFileStream?.Dispose();
            _lockFileStream = null;

            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disposing temporary directory: {ex.Message}");
        }
    }
}
