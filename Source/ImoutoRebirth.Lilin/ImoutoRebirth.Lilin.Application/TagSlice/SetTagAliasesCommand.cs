using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Lilin.Application.TagSlice;

public record SetTagAliasesCommand(Guid TagId, IReadOnlyCollection<Guid> AliasTagIds) : ICommand;
