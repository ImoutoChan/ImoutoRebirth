namespace ImoutoRebirth.Room.Domain;

public class SystemFileMoved : SystemFilePreparedToMove
{
    internal SystemFileMoved(
        SystemFile file, 
        bool requireSave,
        FileInfo movedFileInfo,
        string? error)
        : base(file)
    {
        RequireSave = requireSave;
        MovedFileInfo = movedFileInfo;
        Error = error;
    }

    public bool RequireSave { get; }

    public FileInfo MovedFileInfo { get; }
    
    public string? Error { get; }
}
