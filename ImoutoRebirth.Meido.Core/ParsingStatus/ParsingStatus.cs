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

        public string? ErrorMessage { get; private set; }

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
            if (!ValidateStatus(Status.SearchFound))
                return;

            Status = Status.SearchFound;
            FileIdFromSource = fileIdFromSource;
            UpdatedAt = DateTimeOffset.Now;
        }

        public void SetSearchNotFound()
        {
            if (!ValidateStatus(Status.SearchNotFound))
                return;

            Status = Status.SearchNotFound;
            UpdatedAt = DateTimeOffset.Now;

            Add(new MetadataNotFound(this));
        }

        public void SetSearchFailed(string errorMessage)
        {
            ArgumentValidator.NotNullOrWhiteSpace(() => errorMessage);

            if (!ValidateStatus(Status.SearchFailed))
                return;

            Status = Status.SearchFailed;
            ErrorMessage = errorMessage;
            UpdatedAt = DateTimeOffset.Now;
        }

        public void SetOriginalRequested()
        {
            if (!ValidateStatus(Status.OriginalRequested))
                return;

            Status = Status.OriginalRequested;
            UpdatedAt = DateTimeOffset.Now;
        }

        public void SetSearchSaved()
        {
            if (!ValidateStatus(Status.SearchSaved))
                return;

            Status = Status.SearchSaved;
            UpdatedAt = DateTimeOffset.Now;
        }

        public void RequestMetadataUpdate()
        {
            if (!ValidateStatus(Status.UpdateRequested))
                return;

            Status = Status.UpdateRequested;
            UpdatedAt = DateTimeOffset.Now;
            Add(new UpdateRequested(this));
        }

        private bool ValidateStatus(Status newStatus)
        {
            if (IsStatusChangeAllowed(newStatus)) 
                return true;

            Add(new DisallowedStatusTransfer(this, newStatus));
            return false;

        }

        private bool IsStatusChangeAllowed(Status newStatus)
        {
            var allowed = (Status, newStatus) switch
            {
                (Status.SearchRequested, Status.SearchFound) => true,
                (Status.SearchRequested, Status.SearchNotFound) => true,
                (Status.SearchRequested, Status.SearchFailed) => true,
                (Status.SearchRequested, Status.SearchSaved) => true,
                (Status.SearchRequested, Status.UpdateRequested) => true,

                (Status.SearchFound, Status.SearchSaved) => true,
                (Status.SearchFound, Status.UpdateRequested) => true,
                
                (Status.SearchFailed, Status.SearchRequested) => true,
                (Status.SearchFailed, Status.SearchFound) => true,
                (Status.SearchFailed, Status.SearchNotFound) => true,
                (Status.SearchFailed, Status.SearchSaved) => true,
                (Status.SearchFailed, Status.UpdateRequested) => true,

                (Status.SearchNotFound, Status.OriginalRequested) => true,
                (Status.SearchNotFound, Status.UpdateRequested) => true,

                (Status.SearchSaved, Status.OriginalRequested) => true,
                (Status.SearchSaved, Status.UpdateRequested) => true,

                (Status.UpdateRequested, Status.SearchFound) => true,
                (Status.UpdateRequested, Status.SearchFailed) => true,
                (Status.UpdateRequested, Status.SearchNotFound) => true,
                (Status.UpdateRequested, Status.SearchSaved) => true,
                _ => false
            };

            return allowed;
        }
    }
}
