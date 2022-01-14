namespace ImoutoRebirth.Lilin.WebApi.Responses
{
    public class FileInfoResponse
    {
        public IReadOnlyCollection<FileTagResponse> Tags { get; }

        public IReadOnlyCollection<FileNoteResponse> Notes { get; }

        public FileInfoResponse(
            IReadOnlyCollection<FileTagResponse> tags, 
            IReadOnlyCollection<FileNoteResponse> notes)
        {
            Tags = tags;
            Notes = notes;
        }
    }
}