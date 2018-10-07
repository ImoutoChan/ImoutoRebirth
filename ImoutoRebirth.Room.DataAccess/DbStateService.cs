using System.Threading.Tasks;
using ImoutoRebirth.Room.Database;

namespace ImoutoRebirth.Room.DataAccess
{
    public class DbStateService : IDbStateService
    {
        private readonly RoomDbContext _roomDbContext;

        public DbStateService(RoomDbContext roomDbContext)
        {
            _roomDbContext = roomDbContext;
        }

        public Task SaveChanges() 
            => _roomDbContext.SaveChangesAsync();
    }
}