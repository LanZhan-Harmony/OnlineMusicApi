using System.Buffers;

namespace OnlineMusicApi.Internal.Infrastructure.Http;

internal static class ResponseReader
{
    public static async Task<byte[]> ReadAsync(
        Stream stream,
        int maxBytes,
        MusicSource source,
        string operation,
        Uri? requestUri,
        CancellationToken cancellationToken
    )
    {
        using MemoryStream buffer = new();
        byte[] rented = ArrayPool<byte>.Shared.Rent(16 * 1024);

        try
        {
            int read;
            while (
                (
                    read = await stream
                        .ReadAsync(rented.AsMemory(), cancellationToken)
                        .ConfigureAwait(false)
                ) > 0
            )
            {
                if (buffer.Length + read > maxBytes)
                {
                    throw new OnlineMusicApiResponseException(
                        "站点响应过大。",
                        operation,
                        source,
                        requestUri
                    );
                }

                buffer.Write(rented, 0, read);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }

        return buffer.ToArray();
    }
}
