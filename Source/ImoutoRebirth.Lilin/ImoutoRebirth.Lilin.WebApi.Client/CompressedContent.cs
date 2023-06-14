using System.IO.Compression;
using System.Net;

namespace ImoutoRebirth.Lilin.WebApi.Client;

public class CompressedContent : HttpContent
{
    private readonly HttpContent _originalContent;
    private readonly CompressionLevel _compressionLevel;

    public CompressedContent(HttpContent content, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        _originalContent = content;
        _compressionLevel = compressionLevel;

        foreach (var header in _originalContent.Headers)
        {
            Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        Headers.ContentEncoding.Add("gzip");
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        await using var gzipStream = new GZipStream(stream, _compressionLevel, leaveOpen: true);
        await _originalContent.CopyToAsync(gzipStream).ConfigureAwait(false);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = -1;
        return false;
    }
}
