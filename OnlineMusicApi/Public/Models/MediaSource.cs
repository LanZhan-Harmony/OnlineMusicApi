namespace OnlineMusicApi;

/// <summary>播放源信息。</summary>
public sealed class MediaSource
{
    /// <summary>播放地址。</summary>
    public required Uri Url { get; init; }

    /// <summary>请求的音质。</summary>
    public AudioQuality RequestedQuality { get; init; }

    /// <summary>实际返回的音质。源降级时与 <see cref="RequestedQuality"/> 不同。</summary>
    public AudioQuality ActualQuality { get; init; }

    /// <summary>编码格式，例如 mp3、flac。</summary>
    public string? Codec { get; init; }

    /// <summary>码率，单位 kbps。</summary>
    public int? BitrateKbps { get; init; }

    /// <summary>地址过期时间，为空表示源未提供。</summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>播放时必须附带的请求头，例如 Referer、User-Agent。</summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } =
        new Dictionary<string, string>();
}
