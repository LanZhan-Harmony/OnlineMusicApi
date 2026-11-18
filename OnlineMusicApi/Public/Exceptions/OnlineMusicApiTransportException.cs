namespace OnlineMusicApi;

/// <summary>网络或超时失败，可安全重试。</summary>
/// <remarks>初始化异常。</remarks>
public sealed class OnlineMusicApiTransportException(
    string message,
    string operation,
    MusicSource source,
    Uri? requestUri = null,
    Exception? innerException = null
)
    : OnlineMusicApiException(
        message,
        operation,
        source,
        requestUri,
        isTransient: true,
        innerException: innerException
    );
