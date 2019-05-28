namespace ImoutoRebirth.Arachne.MessageContracts.Commands
{
    public interface ILoadTagHistoryCommand
    {
        SearchEngineType SearchEngineType { get; }

        int LastProcessedTagHistoryId { get; }
    }
}