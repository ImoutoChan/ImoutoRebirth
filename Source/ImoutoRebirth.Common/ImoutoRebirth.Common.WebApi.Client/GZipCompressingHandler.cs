namespace ImoutoRebirth.Common.WebApi.Client;

public class GZipCompressingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken ct)
    {
        if (request.Content != null && request.Content is not CompressedContent)
            request.Content = new CompressedContent(request.Content);

        return await base.SendAsync(request, ct);
    }
}
