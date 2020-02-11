using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    [CommandQuery(IsolationLevel.ReadCommitted)]
    public class UnbindTagCommand : ICommand
    {
        public FileTagInfo FileTag { get; }

        public UnbindTagCommand(FileTagInfo fileTag)
        {
            FileTag = fileTag;
        }
    }
}