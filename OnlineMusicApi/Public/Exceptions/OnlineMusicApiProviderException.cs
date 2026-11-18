namespace OnlineMusicApi;

/// <summary>上游返回明确的业务错误码。</summary>
/// <remarks>初始化异常。</remarks>
public class OnlineMusicApiProviderException(
    string message,
    string operation,
    MusicSource source,
    string? providerCode = null,
    bool isTransient = false,
    TimeSpan? retryAfter = null,
    Exception? innerException = null
)
    : OnlineMusicApiException(
        message,
        operation,
        source,
        providerCode: providerCode,
        isTransient: isTransient,
        retryAfter: retryAfter,
        innerException: innerException
    );
