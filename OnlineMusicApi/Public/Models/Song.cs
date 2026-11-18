namespace OnlineMusicApi;

/// <summary>歌曲。搜索结果、专辑歌曲、歌单歌曲均使用此模型。</summary>
public sealed class Song : ISourcedEntity
{
    /// <summary>歌曲标识，内含音源。</summary>
    public required SongId Id { get; init; }

    /// <summary>歌曲标题。</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>艺术家简要列表。</summary>
    public IReadOnlyList<ArtistRef> Artists { get; init; } = [];

    /// <summary>所属专辑简要信息。</summary>
    public AlbumRef? Album { get; init; }

    /// <summary>时长。</summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>封面地址。</summary>
    public string CoverUrl { get; init; } = string.Empty;

    /// <summary>发行年份。</summary>
    public int? Year { get; init; }

    /// <summary>可播放状态。搜索结果中为 <see cref="PlaybackAvailability.Playable"/>。</summary>
    public PlaybackAvailability Availability { get; init; } = PlaybackAvailability.Unknown;

    /// <summary>所属音源，等价于 <c>Id.Source</c>。</summary>
    public MusicSource Source => Id.Source;
}
