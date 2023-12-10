using System.Net.Http.Json;

namespace ImoutoRebirth.Common.Tests;

public static class HttpClientExtensions
{
    public static async Task<T> ReadResult<T>(this Task<HttpResponseMessage> responseTask)
    {
        var response = await responseTask;

        response.EnsureSuccessStatusCode();
        
        var tValue = await response.Content.ReadFromJsonAsync<T>();
        
        if (tValue == null)
            throw new InvalidOperationException("Response is null");
        
        return tValue;
    }
}
