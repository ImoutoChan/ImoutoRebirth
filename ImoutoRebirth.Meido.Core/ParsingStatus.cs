using System;

namespace ImoutoRebirth.Meido.Core
{
    public class ParsingStatus
    {
        public Guid FileId { get; }

        public string Md5 { get; }

        public int SourceId { get; }

        public int FileIdFromSource { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public Status Status { get; private set; }

        public string ErrorMessage { get; private set; }

        private ParsingStatus(
            Guid fileId,
            string md5,
            int sourceId,
            DateTimeOffset updatedAt,
            Status status)
        {
            FileId = fileId;
            Md5 = md5;
            UpdatedAt = updatedAt;
            Status = status;
            SourceId = sourceId;
        }

        public static ParsingStatus Create(Guid fileId, string md5, int sourceId) 
            => new ParsingStatus(fileId, md5, sourceId, DateTimeOffset.Now, Status.SearchRequested);

        public void SetSearchFound(int fileIdFromSource)
        {
            Status = Status.SearchFound;
            FileIdFromSource = fileIdFromSource;
            UpdatedAt = DateTimeOffset.Now;
        }

        public void SetSearchNotFound()
        {
            Status = Status.SearchNotFound;
            UpdatedAt = DateTimeOffset.Now;
        }

        public void SetSearchFailed(string errorMessage)
        {
            Status = Status.SearchFailed;
            ErrorMessage = errorMessage;
            UpdatedAt = DateTimeOffset.Now;
        }

        public void SetOriginalRequested()
        {
            Status = Status.OriginalRequested;
            UpdatedAt = DateTimeOffset.Now;
        }

        public void SetSearchSaved()
        {
            Status = Status.SearchSaved;
            UpdatedAt = DateTimeOffset.Now;
        }
    }
}
