using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.Core.Services
{
    public interface IDestinationFolderService
    {
        MovedInformation Move(DestinationFolder destinationFolder, MoveInformation moveInformation);
    }
}