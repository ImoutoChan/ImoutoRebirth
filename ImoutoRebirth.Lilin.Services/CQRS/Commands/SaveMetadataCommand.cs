using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.MessageContracts;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    public class SaveMetadataCommand : ICommand
    {
        public IUpdateMetadataCommand MqCommand { get; }

        public SaveMetadataCommand(IUpdateMetadataCommand mqCommand)
        {
            MqCommand = mqCommand;
        }
    }
}