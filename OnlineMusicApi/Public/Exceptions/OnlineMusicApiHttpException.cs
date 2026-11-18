namespace OnlineMusicApi;

/// <summary>上游返回非 2xx 状态码。</summary>
/// <remarks>初始化异常。</remarks>
public sealed class OnlineMusicApiHttpException(
    string message,
    string operation,
    MusicSource source,
    int statusCode,
    Uri? requestUri = null,
    bool isTransient = false,
    Exception? innerException = null
)
    : OnlineMusicApiException(
        message,
        operation,
        source,
        requestUri,
        statusCode,
        isTransient: isTransient,
        innerException: innerException
    );
