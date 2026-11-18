namespace OnlineMusicApi;

/// <summary>JSON 反序列化失败。</summary>
/// <remarks>初始化异常。</remarks>
public sealed class OnlineMusicApiDeserializationException(
    string message,
    string operation,
    MusicSource source,
    Uri? requestUri = null,
    Exception? innerException = null
) : OnlineMusicApiException(message, operation, source, requestUri, innerException: innerException);
