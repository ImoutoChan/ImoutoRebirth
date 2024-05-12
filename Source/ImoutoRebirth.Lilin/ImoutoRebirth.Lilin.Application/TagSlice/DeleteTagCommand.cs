using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Lilin.Application.TagSlice;

public record DeleteTagCommand(Guid TagToDeleteId) : ICommand;
