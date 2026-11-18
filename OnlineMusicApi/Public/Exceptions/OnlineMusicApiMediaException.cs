namespace OnlineMusicApi;

/// <summary>播放地址或歌词解密失败。</summary>
/// <remarks>初始化异常。</remarks>
public sealed class OnlineMusicApiMediaException(
    string message,
    string operation,
    MusicSource source,
    Exception? innerException = null
) : OnlineMusicApiException(message, operation, source, innerException: innerException);
