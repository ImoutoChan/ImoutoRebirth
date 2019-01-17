using ImoutoRebirth.Lilin.MessageContracts;

namespace ImoutoRebirth.Arachne.Service.Commands
{
    public class FileNote : IFileNote
    {
        public int? SourceId { get; set; }

        public string Label { get; set; }

        public int PositionFromLeft { get; set; }

        public int PositionFromTop { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}