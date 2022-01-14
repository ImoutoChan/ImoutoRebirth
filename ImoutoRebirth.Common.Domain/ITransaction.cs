namespace ImoutoRebirth.Common.Domain;

public interface ITransaction : IDisposable
{
    Task CommitAsync();
}