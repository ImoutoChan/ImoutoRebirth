using ImoutoRebirth.Lilin.Core.FileInfoAggregate;
using ImoutoRebirth.Lilin.DataAccess.Entities;

namespace ImoutoRebirth.Lilin.Infrastructure.Mappers;

public static class FileNoteMapper
{
    public static NoteEntity ToNewEntity(this FileNote fileNoteModel) 
        => new()
        {
            FileId = fileNoteModel.FileId,
            Id = Guid.NewGuid(),
            Source = fileNoteModel.Source,
            Label = fileNoteModel.Label,
            SourceId = fileNoteModel.SourceId,
            Height = fileNoteModel.Height,
            Width = fileNoteModel.Width,
            PositionFromLeft = fileNoteModel.PositionFromLeft,
            PositionFromTop = fileNoteModel.PositionFromTop
        };

    public static FileNote ToModel(this NoteEntity entity)
        => new(
            entity.FileId,
            entity.Label,
            entity.PositionFromLeft,
            entity.PositionFromTop,
            entity.Width,
            entity.Height,
            entity.Source,
            entity.SourceId);
}
