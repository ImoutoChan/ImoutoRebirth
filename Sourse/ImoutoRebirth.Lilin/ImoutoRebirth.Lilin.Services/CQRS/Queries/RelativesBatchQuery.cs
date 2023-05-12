using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries;

public class RelativesBatchQuery : IQuery<IReadOnlyCollection<RelativeShortInfo>>
{
    public IReadOnlyCollection<string> Md5 { get; }

    public RelativesBatchQuery(IReadOnlyCollection<string> md5)
    {
        Md5 = md5;
    }
}
