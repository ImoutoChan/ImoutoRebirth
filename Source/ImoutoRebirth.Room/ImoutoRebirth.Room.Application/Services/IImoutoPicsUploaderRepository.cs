using ImoutoRebirth.Room.Domain;

namespace ImoutoRebirth.Room.Application.Services;

public interface IImoutoPicsUploaderRepository
{
    ImoutoPicsUploaderState Get();
}

/// <summary>
/// Should be registered as a singleton.
/// </summary>
public class ImoutoPicsUploaderRepository : IImoutoPicsUploaderRepository
{
    private readonly ImoutoPicsUploaderState _state = new();
    
    public ImoutoPicsUploaderState Get() => _state;
}
