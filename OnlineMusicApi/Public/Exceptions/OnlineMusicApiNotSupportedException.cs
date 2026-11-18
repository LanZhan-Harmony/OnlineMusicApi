namespace OnlineMusicApi;

/// <summary>该音源未实现请求的能力。</summary>
/// <remarks>初始化异常。</remarks>
public sealed class OnlineMusicApiNotSupportedException(
    string operation,
    MusicSource source,
    ProviderCapability capability
) : OnlineMusicApiException($"音源 {source} 不支持能力 {capability}。", operation, source)
{
    /// <summary>缺失的能力。</summary>
    public ProviderCapability Capability { get; } = capability;
}
