using System.Collections.Generic;
using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    [CommandQuery(IsolationLevel.ReadCommitted)]
    public class BindTagsCommand : ICommand
    {
        public IReadOnlyCollection<FileTagInfo> FileTags { get; }

        public BindTagsCommand(IReadOnlyCollection<FileTagInfo> fileTags)
        {
            FileTags = fileTags;
        }
    }
}