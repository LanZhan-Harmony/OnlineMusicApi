namespace OnlineMusicApi;

/// <summary>音频质量档位。源不支持时降级到最近可用档，并通过 <c>MediaSource.ActualQuality</c> 回填。</summary>
public enum AudioQuality
{
    /// <summary>128kbps / 标准。</summary>
    Low = 0,

    /// <summary>192kbps。</summary>
    Medium = 1,

    /// <summary>320kbps。</summary>
    High = 2,

    /// <summary>无损（FLAC 等）。</summary>
    Lossless = 3,

    /// <summary>源支持的最高档，用于搜索探测。</summary>
    Highest = 4,
}
