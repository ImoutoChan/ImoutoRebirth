namespace ImoutoRebirth.Room.Domain;

public class ImoutoPicsUploaderState
{
    public bool IsEnabled { get; private set; } = true;

    public void Enable() => IsEnabled = true;
    
    public void Disable() => IsEnabled = false;
}
