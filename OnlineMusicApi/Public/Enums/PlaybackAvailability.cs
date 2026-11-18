namespace OnlineMusicApi;

/// <summary>歌曲的可播放状态。</summary>
public enum PlaybackAvailability
{
    /// <summary>未探测（例如探测开关关闭、或非歌曲列表场景）。</summary>
    Unknown = 0,

    /// <summary>已探测可播放。</summary>
    Playable = 1,

    /// <summary>不可播放。探测并剔除后不会出现在返回列表中。</summary>
    Unavailable = 2,
}
