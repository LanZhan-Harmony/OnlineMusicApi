namespace OnlineMusicApi;

/// <summary>登录态缺失或触发风控。</summary>
/// <remarks>初始化异常。</remarks>
public sealed class OnlineMusicApiAuthenticationException(
    string message,
    string operation,
    MusicSource source,
    string? providerCode = null,
    Exception? innerException = null
)
    : OnlineMusicApiProviderException(
        message,
        operation,
        source,
        providerCode,
        innerException: innerException
    );
