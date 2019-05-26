using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models;

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