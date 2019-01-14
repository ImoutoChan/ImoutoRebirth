using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.DataAccess.Entities;

namespace ImoutoRebirth.Lilin.Infrastructure.Mappers
{
    public static class FileTagMapper
    {
        public static FileTagEntity ToEntity(this FileTag fileTagModel) 
            => new FileTagEntity
            {
                FileId = fileTagModel.FileId,
                TagId = fileTagModel.Tag.Id,
                Source = fileTagModel.Source,
                Value = fileTagModel.Value
            };

        public static FileTagEntity ToEntity(this FileTagBind model) 
            => new FileTagEntity
            {
                FileId = model.FileId,
                TagId = model.TagId,
                Source = model.Source,
                Value = model.Value
            };

        public static FileTag ToModel(this FileTagEntity entity)
            => new FileTag(entity.FileId, entity.Tag.ToModel(), entity.Value, entity.Source);
    }
}