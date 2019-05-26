using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Meido.MessageContracts;
using System;

namespace ImoutoRebirth.Meido.Services.Cqrs.Commands
{
    internal class SearchCompleteCommand : ICommand
    {
        public SearchCompleteCommand(
            int sourceId,
            Guid fileId, 
            SearchStatus resultStatus, 
            string errorText, 
            int? fileIdFromSource)
        {
            SourceId = sourceId;
            ResultStatus = resultStatus;
            ErrorText = errorText;
            FileIdFromSource = fileIdFromSource;
            FileId = fileId;
        }

        public Guid FileId { get; }

        public int SourceId { get; }

        public SearchStatus ResultStatus { get; }

        public string ErrorText { get; }

        public int? FileIdFromSource { get; }
    }
}