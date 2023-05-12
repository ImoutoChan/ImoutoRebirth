using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries;

public class RelativesQuery : IQuery<IReadOnlyCollection<RelativeInfo>>
{
    public string Md5 { get; }

    public RelativesQuery(string md5)
    {
        Md5 = md5;
    }
}