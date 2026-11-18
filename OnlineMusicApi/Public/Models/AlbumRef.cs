namespace OnlineMusicApi;

/// <summary>专辑简要引用。</summary>
public sealed class AlbumRef
{
    /// <summary>专辑标识。</summary>
    public required AlbumId Id { get; init; }

    /// <summary>专辑名称。</summary>
    public string Title { get; init; } = string.Empty;
}
