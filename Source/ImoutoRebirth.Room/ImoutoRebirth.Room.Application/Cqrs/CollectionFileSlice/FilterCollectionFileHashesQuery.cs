using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;

public record FilterCollectionFileHashesQuery(IReadOnlyCollection<string> Md5Hashes) 
    : IQuery<IReadOnlyCollection<string>>;
