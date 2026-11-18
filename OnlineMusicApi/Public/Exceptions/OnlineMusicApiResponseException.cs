namespace OnlineMusicApi;

/// <summary>JSON 合法，但业务结构不符合预期。</summary>
/// <remarks>初始化异常。</remarks>
public sealed class OnlineMusicApiResponseException(
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
        innerException: innerException
    );
