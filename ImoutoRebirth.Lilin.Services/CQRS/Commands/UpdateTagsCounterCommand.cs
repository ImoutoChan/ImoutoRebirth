using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    [Command(IsolationLevel.ReadCommitted)]
    public class UpdateTagsCountersCommand : ICommand
    {
    }
}