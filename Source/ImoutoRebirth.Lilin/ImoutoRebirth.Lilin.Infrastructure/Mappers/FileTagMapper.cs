﻿using ImoutoRebirth.Lilin.Domain.FileInfoAggregate;
using ImoutoRebirth.Lilin.DataAccess.Entities;

namespace ImoutoRebirth.Lilin.Infrastructure.Mappers;

public static class FileTagMapper
{
    public static FileTagEntity ToNewEntity(this FileTag fileTagModel) 
        => new()
        {
            Id = Guid.NewGuid(),
            FileId = fileTagModel.FileId,
            TagId = fileTagModel.TagId,
            Source = fileTagModel.Source,
            Value = fileTagModel.Value
        };

    public static FileTag ToModel(this FileTagEntity entity) 
        => new(entity.FileId, entity.TagId, entity.Value, entity.Source);
}
