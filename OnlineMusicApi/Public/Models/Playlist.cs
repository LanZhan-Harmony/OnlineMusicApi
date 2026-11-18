namespace OnlineMusicApi;

/// <summary>歌单。仅用于搜索结果，不提供单独的歌单详情接口。</summary>
public sealed class Playlist : ISourcedEntity
{
    /// <summary>歌单标识，内含音源。</summary>
    public required PlaylistId Id { get; init; }

    /// <summary>歌单名称。</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>歌曲数量。</summary>
    public int? SongCount { get; init; }

    /// <summary>歌单介绍。</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>封面地址。</summary>
    public string CoverUrl { get; init; } = string.Empty;

    /// <summary>所属音源，等价于 <c>Id.Source</c>。</summary>
    public MusicSource Source => Id.Source;
}
