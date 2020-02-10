using System;
using ImoutoRebirth.Common;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using ImoutoRebirth.Lilin.DataAccess.Entities;

namespace ImoutoRebirth.Lilin.Infrastructure.Mappers
{
    public static class FileTagMapper
    {
        public static FileTagEntity ToEntity(this FileTag fileTagModel) 
            => new FileTagEntity
            {
                Id = Guid.NewGuid(),
                FileId = fileTagModel.FileId,
                TagId = fileTagModel.Tag.Id,
                Source = fileTagModel.Source,
                Value = fileTagModel.Value
            };

        public static FileTagEntity ToEntity(this FileTagInfo fileTagInfoModel) 
            => new FileTagEntity
            {
                Id = Guid.NewGuid(),
                FileId = fileTagInfoModel.FileId,
                TagId = fileTagInfoModel.TagId,
                Source = fileTagInfoModel.Source,
                Value = fileTagInfoModel.Value
            };

        public static FileTag ToModel(this FileTagEntity entity)
        {
            ArgumentValidator.NotNull(entity.Tag, nameof(entity.Tag));

            return new FileTag(entity.FileId, entity.Tag.ToModel(), entity.Value, entity.Source);
        }
    }
}