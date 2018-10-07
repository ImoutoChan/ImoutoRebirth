using System.Threading.Tasks;

namespace ImoutoRebirth.Room.DataAccess
{
    public interface IDbStateService
    {
        Task SaveChanges();
    }
}