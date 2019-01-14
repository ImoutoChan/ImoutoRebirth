using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Services.CQRS.Abstract;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    public class SaveMetadataCommand : ICommand
    {
        public MetadataUpdate Update { get; }

        public SaveMetadataCommand(MetadataUpdate update)
        {
            Update = update;
        }
    }
}