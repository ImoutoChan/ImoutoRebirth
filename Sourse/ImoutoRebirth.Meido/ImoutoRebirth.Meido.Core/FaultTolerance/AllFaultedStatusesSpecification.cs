using System.Linq.Expressions;
using ImoutoRebirth.Common.Domain.Specifications;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using NodaTime;

namespace ImoutoRebirth.Meido.Core.FaultTolerance;

public class AllFaultedStatusesSpecification : Specification<ParsingStatus.ParsingStatus>
{
    private static readonly Instant CheckDate 
        = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(1));

    public override Expression<Func<ParsingStatus.ParsingStatus, bool>> ToExpression()
    {
        return x => StatusSet.AllFaulted.Contains(x.Status) && x.UpdatedAt < CheckDate;
    }
}
