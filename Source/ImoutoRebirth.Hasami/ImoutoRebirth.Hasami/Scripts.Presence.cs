namespace ImoutoRebirth.Hasami;

internal static partial class Scripts
{
    /// <summary>
    /// We have two folders:
    /// - working folder with new files, that we want to separate into already present on test folder and not present
    /// - test folder with already present files
    /// </summary>
    public static void SplitBasedOnPresenceInOtherFolder()
    {
        var testFolder = new DirectoryInfo(@"C:\Playground\DodjiMetaSearching\Processed");
        var workingFolder = new DirectoryInfo(@"C:\Playground\DodjiMetaSearching\New");

        var targetFoundPath = new DirectoryInfo(Path.Combine(workingFolder.FullName, "!found"));
        var targetMissPath = new DirectoryInfo(Path.Combine(workingFolder.FullName, "!miss"));

        if (!targetFoundPath.Exists)
            targetFoundPath.Create();

        if (!targetMissPath.Exists)
            targetMissPath.Create();

        var newFiles = workingFolder.GetFiles();
        var newFilesHashes = newFiles.Select(x => (File: x, MD5: GetMd5Checksum(x))).ToList();

        var presentFiles = testFolder.GetFiles("*", SearchOption.AllDirectories);
        var presentHashes = presentFiles.Select(x => (File: x, MD5: GetMd5Checksum(x))).ToList();
        var presentHashesOnly = presentHashes.Select(x => x.MD5).ToList();

        foreach (var newFile in newFilesHashes)
        {
            var isPresent = presentHashesOnly.Contains(newFile.MD5);

            var newPath = isPresent
                ? Path.Combine(targetFoundPath.FullName, newFile.File.Name)
                : Path.Combine(targetMissPath.FullName, newFile.File.Name);

            try
            {
                newFile.File.MoveTo(newPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
