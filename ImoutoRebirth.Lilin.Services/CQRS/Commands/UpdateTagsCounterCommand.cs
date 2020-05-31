using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    [CommandQuery(IsolationLevel.Serializable)]
    public class UpdateTagsCountersCommand : ICommand
    {
    }
}