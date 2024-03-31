using ImoutoRebirth.Room.WebApi.Client;

namespace ImoutoRebirth.Navigator.Services.Collections;

internal class ImoutoPicsUploaderStateService : IImoutoPicsUploaderStateService
{
    private readonly ImoutoPicsUploaderEnabledClient _imoutoPicsUploaderEnabledClient;

    public ImoutoPicsUploaderStateService(ImoutoPicsUploaderEnabledClient imoutoPicsUploaderEnabledClient) 
        => _imoutoPicsUploaderEnabledClient = imoutoPicsUploaderEnabledClient;

    public async Task EnableAsync() 
        => await _imoutoPicsUploaderEnabledClient.EnableImoutoPicsUploaderAsync();

    public async Task DisableAsync() 
        => await _imoutoPicsUploaderEnabledClient.DisableImoutoPicsUploaderAsync();

    public async Task<bool> IsEnabledAsync() 
        => await _imoutoPicsUploaderEnabledClient.IsImoutoPicsUploaderEnabledAsync();
}
