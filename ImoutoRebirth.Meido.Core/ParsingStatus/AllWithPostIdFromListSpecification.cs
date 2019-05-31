using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Domain.Specifications;

namespace ImoutoRebirth.Meido.Core.ParsingStatus
{
    public class AllWithPostIdFromListSpecification : Specification<ParsingStatus>
    {
        private readonly List<int?> _postIds;
        private readonly MetadataSource _source;

        public AllWithPostIdFromListSpecification(IReadOnlyCollection<int> postIds, MetadataSource source)
        {
            ArgumentValidator.IsEnumDefined(() => source);
            ArgumentValidator.Requires(postIds.Any, nameof(postIds));

            _postIds = postIds.Cast<int?>().ToList();
            _source = source;
        }

        public override Expression<Func<ParsingStatus, bool>> ToExpression()
        {
            return x => x.Source == _source 
                        && x.FileIdFromSource != null 
                        && _postIds.Contains(x.FileIdFromSource);
        }
    }
}