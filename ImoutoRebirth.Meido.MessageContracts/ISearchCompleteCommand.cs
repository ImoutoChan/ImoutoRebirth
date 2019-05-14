using System;

namespace ImoutoRebirth.Meido.MessageContracts
{
    public interface ISearchCompleteCommand
    {
        Guid FileId { get; }

        int SourceId { get; }

        SearchStatus ResultStatus { get; }

        string ErrorText { get; }

        int FileIdFromSource { get; }
    }
}