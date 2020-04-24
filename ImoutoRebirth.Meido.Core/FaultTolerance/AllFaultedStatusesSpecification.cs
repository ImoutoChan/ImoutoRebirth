using System;
using System.Linq;
using System.Linq.Expressions;
using ImoutoRebirth.Common.Domain.Specifications;
using ImoutoRebirth.Meido.Core.ParsingStatus;

namespace ImoutoRebirth.Meido.Core.FaultTolerance
{
    public class AllFaultedStatusesSpecification : Specification<ParsingStatus.ParsingStatus>
    {
        public override Expression<Func<ParsingStatus.ParsingStatus, bool>> ToExpression()
        {
            return x => StatusSet.AllFaulted.Contains(x.Status) 
                        &&  DateTimeOffset.Now - x.UpdatedAt > TimeSpan.FromDays(1);
        }
    }
}