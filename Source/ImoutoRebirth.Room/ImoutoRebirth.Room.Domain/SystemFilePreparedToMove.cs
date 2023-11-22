namespace ImoutoRebirth.Room.Domain;

public class SystemFilePreparedToMove
{
    public SystemFilePreparedToMove(SystemFile systemFile) => SystemFile = systemFile;

    public SystemFile SystemFile { get; }

    public MoveProblem MoveProblem { get; private set; }

    public IReadOnlyCollection<string> SourceTags { get; private set; } = Array.Empty<string>();
    
    public void SetProblem(MoveProblem moveProblem)
    {
        MoveProblem = moveProblem;
    }
    
    public void AddSourceTags(IReadOnlyCollection<string> sourceTags)
    {
        SourceTags = SourceTags.Union(sourceTags).ToList();
    }
    
    public SystemFileMoved CreateMoved(bool requireSave, FileInfo movedFileInfo)
    {
        var created = new SystemFileMoved(SystemFile, requireSave, movedFileInfo, null);
        created.SetProblem(MoveProblem);
        created.AddSourceTags(SourceTags);
        return created;
    }
    
    public SystemFileMoved CreateMovedSuccess(FileInfo movedFileInfo)
    {
        var created = new SystemFileMoved(SystemFile, true, movedFileInfo, null);
        created.SetProblem(MoveProblem);
        created.AddSourceTags(SourceTags);
        return created;
    }
    
    public SystemFileMoved CreateMovedFail(string error)
    {
        var created = new SystemFileMoved(SystemFile, false, SystemFile.File, error);
        created.SetProblem(MoveProblem);
        created.AddSourceTags(SourceTags);
        return created;
    }
}

public enum MoveProblem
{
    None,
    InvalidFormat,
    WithoutHash,
    IncorrectHash,
    AlreadyContains
}
