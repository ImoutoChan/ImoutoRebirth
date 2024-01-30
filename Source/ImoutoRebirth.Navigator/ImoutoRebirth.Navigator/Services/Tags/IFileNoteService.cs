using LocalFileNote = ImoutoRebirth.Navigator.Services.Tags.Model.FileNote;

namespace ImoutoRebirth.Navigator.Services.Tags;

internal interface IFileNoteService
{
    Task<IReadOnlyCollection<LocalFileNote>> GetFileNotes(Guid fileId);
}
