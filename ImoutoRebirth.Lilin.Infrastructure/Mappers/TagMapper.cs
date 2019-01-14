using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.DataAccess.Entities;

namespace ImoutoRebirth.Lilin.Infrastructure.Mappers
{
    public static class TagMapper
    {
        public static Tag ToModel(this TagEntity entity)
            => new Tag(
                entity.Id,
                entity.Type.ToModel(),
                entity.Name,
                entity.HasValue,
                entity.SynonymsArray,
                entity.Count);
    }
}