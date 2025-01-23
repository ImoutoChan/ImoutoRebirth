using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Domain;
using NodaTime;

namespace ImoutoRebirth.Meido.Domain.ParsingStatusAggregate;

public class ParsingStatus
{
    public Guid FileId { get; }

    public string Md5 { get; }

    public MetadataSource Source { get; }

    public string? FileIdFromSource { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public Status Status { get; private set; }

    public string? ErrorMessage { get; private set; }

    private ParsingStatus(
        Guid fileId,
        string md5,
        MetadataSource source,
        Instant updatedAt,
        Status status)
    {
        FileId = fileId;
        Md5 = md5;
        UpdatedAt = updatedAt;
        Status = status;
        Source = source;
    }

    public static DomainResult<ParsingStatus> Create(Guid fileId, string md5, MetadataSource source, Instant now)
    {
        ArgumentValidator.Requires(() => fileId != Guid.Empty, nameof(fileId));
        ArgumentValidator.NotNullOrWhiteSpace(() => md5);
        ArgumentValidator.IsEnumDefined(() => source);

        var created = new ParsingStatus(fileId, md5, source, now, Status.SearchRequested);

        return new DomainResult<ParsingStatus>(created)
        {
            new ParsingStatusCreated(created)
        };
    }

    public DomainResult SetSearchResult(SearchStatus result, string? fileIdFromSource, string? error, Instant now)
    {
        return result switch
        {
            SearchStatus.NotFound => SetSearchNotFound(now),
            SearchStatus.Success => SetSearchFound(fileIdFromSource, now),
            SearchStatus.Error => SetSearchFailed(error, now),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
    }
    
    
    private DomainResult SetSearchFound(string? fileIdFromSource, Instant now)
    {
        ArgumentValidator.NotNull(fileIdFromSource, nameof(fileIdFromSource));
        
        var validated = ValidateStatus(Status.SearchFound);
        if (!validated)
            return new DomainResult(new DisallowedStatusTransfer(this, Status.SearchFound));

        Status = Status.SearchFound;
        FileIdFromSource = fileIdFromSource;
        UpdatedAt = now;

        return DomainResult.Empty;
    }

    private DomainResult SetSearchNotFound(Instant now)
    {
        var validated = ValidateStatus(Status.SearchNotFound);
        if (!validated)
            return new DomainResult(new DisallowedStatusTransfer(this, Status.SearchNotFound));

        Status = Status.SearchNotFound;
        UpdatedAt = now;

        return new DomainResult(new MetadataNotFound(this));
    }

    private DomainResult SetSearchFailed(string? errorMessage, Instant now)
    {
        ArgumentValidator.NotNullOrWhiteSpace(() => errorMessage!);

        var validated = ValidateStatus(Status.SearchFailed);
        if (!validated)
            return new DomainResult(new DisallowedStatusTransfer(this, Status.SearchFailed));

        Status = Status.SearchFailed;
        ErrorMessage = errorMessage;
        UpdatedAt = now;

        return DomainResult.Empty;
    }

    public DomainResult SetOriginalRequested(Instant now)
    {
        var validated = ValidateStatus(Status.OriginalRequested);
        if (!validated)
            return new DomainResult(new DisallowedStatusTransfer(this, Status.OriginalRequested));

        Status = Status.OriginalRequested;
        UpdatedAt = now;

        return DomainResult.Empty;
    }

    public DomainResult SetSearchSaved(Instant now)
    {
        var validated = ValidateStatus(Status.SearchSaved);
        if (!validated)
            return new DomainResult(new DisallowedStatusTransfer(this, Status.SearchSaved));

        Status = Status.SearchSaved;
        UpdatedAt = now;
        
        return DomainResult.Empty;
    }

    public DomainResult RequestMetadataUpdate(Instant now)
    {
        var validated = ValidateStatus(Status.UpdateRequested);
        if (!validated)
            return new DomainResult(new DisallowedStatusTransfer(this, Status.UpdateRequested));

        Status = Status.UpdateRequested;
        UpdatedAt = now;

        return new DomainResult(new UpdateRequested(this));
    }

    private bool ValidateStatus(Status newStatus) => IsStatusChangeAllowed(newStatus);

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
            (Status.UpdateRequested, Status.UpdateRequested) => true,
            _ => false
        };

        return allowed;
    }
}

public enum SearchStatus
{
    NotFound = 0,
    Success = 1,
    Error = 2
}
