namespace OnlineMusicApi;

/// <summary>专辑。搜索结果与 <c>GetAlbumAsync</c> 共用此模型。</summary>
public sealed class Album : ISourcedEntity
{
    /// <summary>专辑标识，内含音源。</summary>
    public required AlbumId Id { get; init; }

    /// <summary>专辑名称。</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>艺术家简要列表。</summary>
    public IReadOnlyList<ArtistRef> Artists { get; init; } = [];

    /// <summary>发行年份。</summary>
    public int? Year { get; init; }

    /// <summary>歌曲数量。</summary>
    public int? SongCount { get; init; }

    /// <summary>专辑介绍。</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>封面地址。</summary>
    public string CoverUrl { get; init; } = string.Empty;

    /// <summary>所属音源，等价于 <c>Id.Source</c>。</summary>
    public MusicSource Source => Id.Source;
}
