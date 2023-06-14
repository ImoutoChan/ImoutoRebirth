using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;

public record RelativesBatchQuery(IReadOnlyCollection<string> Md5) : IQuery<IReadOnlyCollection<RelativeShortInfo>>;

public record RelativeShortInfo(string Hash, RelativeType? RelativeType);
