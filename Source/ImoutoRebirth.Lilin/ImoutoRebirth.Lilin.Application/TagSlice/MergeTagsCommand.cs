using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Lilin.Application.TagSlice;

public record MergeTagsCommand(Guid TagToCleanId, Guid TagToEnrichId) : ICommand;
