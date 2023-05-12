using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Lilin.MessageContracts;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands;

[CommandQuery(IsolationLevel.Serializable)]
public class SaveMetadataCommand : ICommand
{
    public IUpdateMetadataCommand MqCommand { get; }

    public SaveMetadataCommand(IUpdateMetadataCommand mqCommand)
    {
        MqCommand = mqCommand;
    }
}