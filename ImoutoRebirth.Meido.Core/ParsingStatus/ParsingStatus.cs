using System;
using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Meido.Core.ParsingStatus.Events;

namespace ImoutoRebirth.Meido.Core.ParsingStatus
{
    public class ParsingStatus : Entity
    {
        public Guid FileId { get; }

        public string Md5 { get; }

        public MetadataSource Source { get; }

        public int? FileIdFromSource { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public Status Status { get; private set; }

        public string ErrorMessage { get; private set; }

        private ParsingStatus(
            Guid fileId,
            string md5,
            MetadataSource source,
            DateTimeOffset updatedAt,
            Status status)
        {
            FileId = fileId;
            Md5 = md5;
            UpdatedAt = updatedAt;
            Status = status;
            Source = source;
        }

        public static ParsingStatus Create(Guid fileId, string md5, MetadataSource source)
        {
            ArgumentValidator.Requires(() => fileId != default, nameof(fileId));
            ArgumentValidator.NotNullOrWhiteSpace(() => md5);
            ArgumentValidator.IsEnumDefined(() => source);

            var created = new ParsingStatus(fileId, md5, source, DateTimeOffset.Now, Status.SearchRequested);
            created.Add(new ParsingStatusCreated(created));
            return created;
        }

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

            Add(new MetadataNotFound(this));
        }

        public void SetSearchFailed(string errorMessage)
        {
            ArgumentValidator.NotNullOrWhiteSpace(() => errorMessage);

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
