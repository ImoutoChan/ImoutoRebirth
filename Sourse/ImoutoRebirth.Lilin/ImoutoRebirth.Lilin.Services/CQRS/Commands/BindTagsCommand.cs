using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands;

[CommandQuery(IsolationLevel.ReadCommitted)]
public class BindTagsCommand : ICommand
{
    public IReadOnlyCollection<FileTagInfo> FileTags { get; }

    public SameTagHandleStrategy SameTagHandleStrategy { get; }

    public BindTagsCommand(IReadOnlyCollection<FileTagInfo> fileTags, SameTagHandleStrategy sameTagHandleStrategy)
    {
        FileTags = fileTags;
        SameTagHandleStrategy = sameTagHandleStrategy;
    }
}