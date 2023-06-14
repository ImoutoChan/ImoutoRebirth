using ImoutoRebirth.Common;
using ImoutoRebirth.Lilin.Core.FileInfoAggregate;
using ImoutoRebirth.Lilin.DataAccess.Entities;

namespace ImoutoRebirth.Lilin.Infrastructure.Mappers;

public static class FileTagMapper
{
    public static FileTagEntity ToEntity(this FileTag fileTagModel) 
        => new()
        {
            Id = Guid.NewGuid(),
            FileId = fileTagModel.FileId,
            TagId = fileTagModel.TagId,
            Source = fileTagModel.Source,
            Value = fileTagModel.Value
        };

    public static FileTag ToModel(this FileTagEntity entity)
    {
        ArgumentValidator.NotNull(entity.Tag, nameof(entity.Tag));

        return new FileTag(entity.FileId, entity.TagId, entity.Value, entity.Source);
    }
}
