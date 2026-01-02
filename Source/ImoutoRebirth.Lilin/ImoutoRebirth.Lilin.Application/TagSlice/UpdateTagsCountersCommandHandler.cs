using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;

namespace ImoutoRebirth.Lilin.Application.TagSlice;

[CommandQuery(IsolationLevel.ReadCommitted)]
public record UpdateTagsCountersCommand : ICommand;