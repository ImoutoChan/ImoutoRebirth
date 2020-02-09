using System.Threading.Tasks;

namespace ImoutoRebirth.Common.Domain
{
    public class EmptyTransaction : ITransaction
    {
        public void Dispose()
        {
        }

        public Task CommitAsync() => Task.CompletedTask;
    }
}