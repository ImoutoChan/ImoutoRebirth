using System.Collections.Generic;

namespace ImoutoRebirth.Room.Core.Models
{
    public class MoveInformation
    {
        public MoveInformation(SystemFile systemFile)
        {
            SystemFile = systemFile;
        }

        public SystemFile SystemFile { get; }

        public MoveProblem MoveProblem { get; set; } = MoveProblem.None;

        public List<string> SourceTags { get; } = new List<string>();
    }
}