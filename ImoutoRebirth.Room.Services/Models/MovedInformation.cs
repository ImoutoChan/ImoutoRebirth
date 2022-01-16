namespace ImoutoRebirth.Room.Core.Models;

public class MovedInformation : MoveInformation
{
    public MovedInformation(
        MoveInformation moveInformation, 
        bool requireSave,
        FileInfo movedFileInfo)
        : base(moveInformation.SystemFile)
    {
        RequireSave = requireSave;
        MovedFileInfo = movedFileInfo;
        MoveProblem = moveInformation.MoveProblem;

        SourceTags.AddRange(moveInformation.SourceTags);
    }

    public bool RequireSave { get; }

    public FileInfo MovedFileInfo { get; }
}