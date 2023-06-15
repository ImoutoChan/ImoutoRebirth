using ImoutoRebirth.Common;
using ImoutoRebirth.Lilin.Domain.TagAggregate;
using ImoutoRebirth.Lilin.DataAccess.Entities;

namespace ImoutoRebirth.Lilin.Infrastructure.Mappers;

public static class TagMapper
{
    public static Tag ToModel(this TagEntity entity)
    {
        ArgumentValidator.NotNull(entity.Type, nameof(entity.Type));

        return new Tag(
            entity.Id,
            entity.Type.ToModel(),
            entity.Name,
            entity.HasValue,
            entity.SynonymsArray,
            entity.Count);
    }
}
