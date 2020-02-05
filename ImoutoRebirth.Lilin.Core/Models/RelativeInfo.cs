using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Core.Models
{
    public class RelativeInfo
    {
        public RelativeType RelativesType { get; }

        public FileInfo FileInfo { get; }

        public RelativeInfo(RelativeType relativesType, FileInfo fileInfo)
        {
            RelativesType = relativesType;
            FileInfo = fileInfo;
        }
    }
}