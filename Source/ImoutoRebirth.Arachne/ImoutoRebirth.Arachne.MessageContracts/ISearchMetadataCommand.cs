namespace ImoutoRebirth.Arachne.MessageContracts;

public interface ISearchMetadataCommand
{
    string Md5 { get; }

    Guid FileId { get; }
}