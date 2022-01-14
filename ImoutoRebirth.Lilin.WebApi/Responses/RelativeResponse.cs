using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.WebApi.Responses;

public class RelativeResponse
{
    /// <summary>
    ///     Type of relative with md5 from request.
    /// </summary>
    public RelativeType RelativesType { get; }

    /// <summary>
    ///     FileInfo of file in system to which found relative is related.
    /// </summary>
    public FileInfoResponse FileInfo { get; }

    public RelativeResponse(RelativeType relativesType, FileInfoResponse fileInfo)
    {
        RelativesType = relativesType;
        FileInfo = fileInfo;
    }
}