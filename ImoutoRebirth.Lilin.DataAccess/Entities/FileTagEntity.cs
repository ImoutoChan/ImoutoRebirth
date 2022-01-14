using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.DataAccess.Entities;

public class FileTagEntity : EntityBase
{
    public Guid FileId { get; set; }

    public Guid TagId { get; set; }

    public string? Value { get; set; }

    public MetadataSource Source { get; set; }


    public TagEntity? Tag { get; set; }
}