using ImoutoRebirth.Common.Cqrs.Abstract;
using NodaTime;

namespace ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;

public record CollectionFileMetadataQuery(Guid Id) : IQuery<CollectionFileMetadata?>;

public record CollectionFileMetadata(Guid Id, string StoredMd5, Instant AddedOn);
