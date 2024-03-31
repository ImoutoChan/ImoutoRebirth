namespace ImoutoRebirth.Navigator.Services.Collections;

public interface IImoutoPicsUploaderStateService
{
    Task EnableAsync();

    Task DisableAsync();

    Task<bool> IsEnabledAsync();
}
