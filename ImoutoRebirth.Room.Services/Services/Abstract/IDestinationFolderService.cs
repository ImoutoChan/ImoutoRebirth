using ImoutoRebirth.Room.Core.Models;

namespace ImoutoRebirth.Room.Core.Services
{
    public interface IDestinationFolderService
    {
        MovedInformation Move(DestinationFolder destinationFolder, MoveInformation moveInformation);
    }
}