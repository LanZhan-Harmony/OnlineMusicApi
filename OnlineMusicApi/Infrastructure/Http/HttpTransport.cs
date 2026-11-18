namespace OnlineMusicApi.Internal.Infrastructure.Http;

internal sealed class HttpTransport
{
    private readonly HttpClient _httpClient;
    private readonly OnlineMusicApiOptions _options;

    public HttpTransport(HttpClient httpClient, OnlineMusicApiOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Task<byte[]> SendAsync(
        MusicSource source,
        string operation,
        Func<HttpRequestMessage> requestFactory,
        RequestRetryMode retryMode,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(requestFactory);

        int attempts = retryMode == RequestRetryMode.NoRetry ? 1 : _options.RetryCount;
        return RetryPolicy.ExecuteAsync(
            (_, token) => SendOnceAsync(source, operation, requestFactory, token),
            attempts,
            RetryPolicy.IsTransient,
            cancellationToken
        );
    }

    private async Task<byte[]> SendOnceAsync(
        MusicSource source,
        string operation,
        Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken
    )
    {
        using HttpRequestMessage request = requestFactory();
        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken
        );
        timeout.CancelAfter(_options.RequestTimeout);

        using HttpResponseMessage response = await SendRequestAsync(
                source,
                operation,
                request,
                cancellationToken,
                timeout.Token
            )
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            int statusCode = (int)response.StatusCode;
            throw new OnlineMusicApiHttpException(
                $"上游返回 HTTP {statusCode}。",
                operation,
                source,
                statusCode,
                request.RequestUri,
                isTransient: statusCode >= 500 || statusCode == 429
            );
        }

        try
        {
            await using Stream stream = await response
                .Content.ReadAsStreamAsync(timeout.Token)
                .ConfigureAwait(false);

            return await ResponseReader
                .ReadAsync(
                    stream,
                    _options.MaxResponseBytes,
                    source,
                    operation,
                    request.RequestUri,
                    timeout.Token
                )
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new OnlineMusicApiTransportException(
                "请求超时。",
                operation,
                source,
                request.RequestUri,
                exception
            );
        }
    }

    private async Task<HttpResponseMessage> SendRequestAsync(
        MusicSource source,
        string operation,
        HttpRequestMessage request,
        CancellationToken userToken,
        CancellationToken timeoutToken
    )
    {
        try
        {
            return await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException exception) when (!userToken.IsCancellationRequested)
        {
            throw new OnlineMusicApiTransportException(
                "请求超时。",
                operation,
                source,
                request.RequestUri,
                exception
            );
        }
        catch (HttpRequestException exception)
        {
            throw new OnlineMusicApiTransportException(
                "网络请求失败。",
                operation,
                source,
                request.RequestUri,
                exception
            );
        }
    }
}
