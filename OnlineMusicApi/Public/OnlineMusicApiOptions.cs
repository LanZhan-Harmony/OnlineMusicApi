namespace OnlineMusicApi;

/// <summary>客户端配置。所有值在构造客户端时归一化。</summary>
public sealed class OnlineMusicApiOptions
{
    /// <summary>单次请求的截止时间，覆盖响应头与响应体两个阶段。</summary>
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(20);

    /// <summary>可重试失败的最大尝试次数，取值 1~3。</summary>
    public int RetryCount { get; init; } = 2;

    /// <summary>响应体大小上限，取值 16KB~64MB。</summary>
    public int MaxResponseBytes { get; init; } = 8 * 1024 * 1024;

    /// <summary>歌曲列表是否探测播放地址。搜索、专辑歌曲、歌单歌曲统一生效。</summary>
    public bool EnablePlaybackProbe { get; init; } = true;

    /// <summary>逐首探测的并发上限，取值 1~16。</summary>
    public int PlaybackProbeConcurrency { get; init; } = 4;

    /// <summary>单首探测超时。</summary>
    public TimeSpan PlaybackProbeTimeout { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>整页被过滤空时是否继续拉取下一页，直到凑满 Limit 或上游到底。</summary>
    public bool GreedyPageFill { get; init; }

    /// <summary>贪婪续拉的页数安全阀，取值 1~50。</summary>
    public int MaxGreedyPages { get; init; } = 10;

    /// <summary>播放地址短缓存时长；<see cref="TimeSpan.Zero"/> 表示禁用。</summary>
    public TimeSpan PlaybackCacheLifetime { get; init; } = TimeSpan.FromMinutes(10);

    /// <summary>是否缓存全量返回列表的快照，仅 SnapshotPaged 源生效。</summary>
    public bool EnableListSnapshotCache { get; init; } = true;

    /// <summary>列表快照有效期；<see cref="TimeSpan.Zero"/> 表示禁用。</summary>
    public TimeSpan ListSnapshotLifetime { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>全部列表快照允许占用的最大歌曲数，取值 1000~200000。</summary>
    public int ListSnapshotMaxTotalSongs { get; init; } = 20000;

    /// <summary>自定义 User-Agent；为空时使用各音源默认值。</summary>
    public string? UserAgent { get; init; }

    internal OnlineMusicApiOptions Normalize()
    {
        return new OnlineMusicApiOptions
        {
            RequestTimeout =
                RequestTimeout > TimeSpan.Zero ? RequestTimeout : TimeSpan.FromSeconds(20),
            RetryCount = Math.Clamp(RetryCount, 1, 3),
            MaxResponseBytes = Math.Clamp(MaxResponseBytes, 16 * 1024, 64 * 1024 * 1024),
            EnablePlaybackProbe = EnablePlaybackProbe,
            PlaybackProbeConcurrency = Math.Clamp(PlaybackProbeConcurrency, 1, 16),
            PlaybackProbeTimeout =
                PlaybackProbeTimeout > TimeSpan.Zero
                    ? PlaybackProbeTimeout
                    : TimeSpan.FromSeconds(5),
            GreedyPageFill = GreedyPageFill,
            MaxGreedyPages = Math.Clamp(MaxGreedyPages, 1, 50),
            PlaybackCacheLifetime =
                PlaybackCacheLifetime > TimeSpan.Zero ? PlaybackCacheLifetime : TimeSpan.Zero,
            EnableListSnapshotCache = EnableListSnapshotCache,
            ListSnapshotLifetime =
                ListSnapshotLifetime > TimeSpan.Zero ? ListSnapshotLifetime : TimeSpan.Zero,
            ListSnapshotMaxTotalSongs = Math.Clamp(ListSnapshotMaxTotalSongs, 1000, 200_000),
            UserAgent = string.IsNullOrWhiteSpace(UserAgent) ? null : UserAgent,
        };
    }
}
