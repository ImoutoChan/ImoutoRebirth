using AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client;
using FileNote = ImoutoRebirth.Navigator.Services.Tags.Model.FileNote;

namespace ImoutoRebirth.Navigator.Services.Tags;

internal class FileNoteService : IFileNoteService
{
    private readonly FilesClient _filesClient;
    private readonly IMapper _mapper;

    public FileNoteService(FilesClient filesClient, IMapper mapper)
    {
        _filesClient = filesClient;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<FileNote>> GetFileNotes(Guid fileId)
    {
        var fileInfo = await _filesClient.GetFileInfoAsync(fileId);
        return _mapper.Map<IReadOnlyCollection<FileNote>>(fileInfo.Notes ?? []);
    }
}
