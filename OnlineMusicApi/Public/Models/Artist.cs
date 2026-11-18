namespace OnlineMusicApi;

/// <summary>艺术家。搜索结果与 <c>GetArtistAsync</c> 共用此模型。</summary>
public sealed class Artist : ISourcedEntity
{
    /// <summary>艺术家标识，内含音源。</summary>
    public required ArtistId Id { get; init; }

    /// <summary>艺术家名称。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>专辑数量。</summary>
    public int? AlbumCount { get; init; }

    /// <summary>歌曲数量。</summary>
    public int? SongCount { get; init; }

    /// <summary>艺术家介绍。</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>头像地址。</summary>
    public string AvatarUrl { get; init; } = string.Empty;

    /// <summary>所属音源，等价于 <c>Id.Source</c>。</summary>
    public MusicSource Source => Id.Source;
}
