using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using ImoutoRebirth.Lilin.DataAccess.Entities;

namespace ImoutoRebirth.Lilin.Infrastructure.Mappers;

public static class FileNoteMapper
{
    public static NoteEntity ToEntity(this FileNote fileNoteModel) 
        => new NoteEntity
        {
            FileId = fileNoteModel.FileId,
            Id = fileNoteModel.Note.Id,
            Source = fileNoteModel.Source,
            Label = fileNoteModel.Note.Label,
            SourceId = fileNoteModel.SourceId,
            Height = fileNoteModel.Note.Height,
            Width = fileNoteModel.Note.Width,
            PositionFromLeft = fileNoteModel.Note.PositionFromLeft,
            PositionFromTop = fileNoteModel.Note.PositionFromTop
        };

    public static FileNote ToModel(this NoteEntity entity)
        => new FileNote(
            entity.FileId,
            new Note(
                entity.Id,
                entity.Label,
                entity.PositionFromLeft,
                entity.PositionFromTop,
                entity.Width,
                entity.Height),
            entity.Source,
            entity.SourceId);
}