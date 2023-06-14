using ImoutoRebirth.Lilin.Core.TagTypeAggregate;
using ImoutoRebirth.Lilin.DataAccess.Entities;

namespace ImoutoRebirth.Lilin.Infrastructure.Mappers;

public static class TagTypeMapper
{
    public static TagType ToModel(this TagTypeEntity entity)
        => new TagType(entity.Id, entity.Name, entity.Color);
}
