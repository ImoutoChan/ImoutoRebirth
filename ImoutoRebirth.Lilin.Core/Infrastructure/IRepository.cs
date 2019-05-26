using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Lilin.Core.Infrastructure
{
    public interface IRepository
    {
        IUnitOfWork UnitOfWork { get; }
    }
}