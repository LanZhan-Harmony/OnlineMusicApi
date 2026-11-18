namespace OnlineMusicApi;

/// <summary>艺术家简要引用。</summary>
public sealed class ArtistRef
{
    /// <summary>艺术家标识。</summary>
    public required ArtistId Id { get; init; }

    /// <summary>艺术家名称。</summary>
    public string Name { get; init; } = string.Empty;
}
