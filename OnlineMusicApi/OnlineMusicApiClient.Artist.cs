namespace OnlineMusicApi;

public sealed partial class OnlineMusicApiClient
{
    /// <summary>根据 ID 获取艺术家信息。</summary>
    /// <param name="id">艺术家标识。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>艺术家信息。</returns>
    public Task<Artist> GetArtistAsync(ArtistId id, CancellationToken cancellationToken = default)
    {
        return _catalog.GetArtistAsync(id, cancellationToken);
    }

    /// <summary>分页获取艺术家拥有的专辑。</summary>
    /// <param name="id">艺术家标识。</param>
    /// <param name="query">分页请求。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>专辑分页结果。</returns>
    public Task<AlbumPage> GetArtistAlbumsAsync(
        ArtistId id,
        PageQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return _catalog.GetArtistAlbumsAsync(id, query, cancellationToken);
    }
}
