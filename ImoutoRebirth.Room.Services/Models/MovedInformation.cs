namespace ImoutoRebirth.Room.Core.Models
{
    public class MovedInformation : MoveInformation
    {
        public MovedInformation(MoveInformation moveInformation, bool requireSave)
            : base(moveInformation.SystemFile)
        {
            RequireSave = requireSave;
            MoveProblem = moveInformation.MoveProblem;

            SourceTags.AddRange(moveInformation.SourceTags);
        }

        public bool RequireSave { get; }
    }
}