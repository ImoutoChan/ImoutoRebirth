using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    [CommandQuery(IsolationLevel.ReadCommitted)]
    public class UpdateTagsCountersCommand : ICommand
    {
    }
}