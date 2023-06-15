using ImoutoRebirth.Lilin.Domain.TagTypeAggregate;
using ImoutoRebirth.Lilin.DataAccess.Entities;

namespace ImoutoRebirth.Lilin.Infrastructure.Mappers;

public static class TagTypeMapper
{
    public static TagType ToModel(this TagTypeEntity entity) => new(entity.Id, entity.Name, entity.Color);
}
