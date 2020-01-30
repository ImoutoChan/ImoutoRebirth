using System;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate
{
    public class FileNoteBind
    {
        public Guid FileId { get; }

        public string Label { get; }

        public int PositionFromLeft { get; }

        public int PositionFromTop { get; }

        public int Width { get; }

        public int Height { get; }

        public MetadataSource Source { get; }

        public int? SourceId { get; }

        public FileNoteBind(
            Guid fileId,
            string label,
            int positionFromLeft,
            int positionFromTop,
            int width,
            int height,
            MetadataSource source,
            int? sourceId)
        {
            ArgumentValidator.NotNullOrWhiteSpace(() => label);

            FileId = fileId;
            Label = label;
            PositionFromLeft = positionFromLeft;
            PositionFromTop = positionFromTop;
            Width = width;
            Height = height;
            Source = source;
            SourceId = sourceId;
        }
    }
}