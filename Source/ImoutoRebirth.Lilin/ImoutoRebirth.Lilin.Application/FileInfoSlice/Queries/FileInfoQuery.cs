using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.FileInfoAggregate;
using ImoutoRebirth.Lilin.Core.TagAggregate;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;

public record FileInfoQuery(Guid FileId) : IQuery<DetailedFileInfo>;

public record DetailedFileInfo(Guid FileId, IReadOnlyCollection<DetailedFileTag> Tags, IReadOnlyCollection<FileNote> Notes);

public record DetailedFileTag(Guid FileId, Tag Tag, string? Value, MetadataSource Source);
