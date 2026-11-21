using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;

public record FileRatingsQuery(IReadOnlyCollection<Guid> FileIds) : IQuery<IReadOnlyCollection<FileRating>>;

public record FileRating(Guid FileId, bool IsFavorite, int Rating);
