using System.Collections.Generic;
using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries
{
    public class TagTypesQuery : IQuery<IReadOnlyCollection<TagType>>
    {
    }
}