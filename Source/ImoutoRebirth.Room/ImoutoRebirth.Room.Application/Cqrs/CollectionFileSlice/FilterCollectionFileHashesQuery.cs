using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;

namespace ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;

[CommandQuery(noTransaction: true)]
public record FilterCollectionFileHashesQuery(IReadOnlyCollection<string> Md5Hashes)
    : IQuery<IReadOnlyCollection<string>>;
