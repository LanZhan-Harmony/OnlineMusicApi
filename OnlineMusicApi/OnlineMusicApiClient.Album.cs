namespace OnlineMusicApi;

public sealed partial class OnlineMusicApiClient
{
    /// <summary>根据 ID 获取专辑信息。</summary>
    /// <param name="id">专辑标识。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>专辑信息。</returns>
    public Task<Album> GetAlbumAsync(AlbumId id, CancellationToken cancellationToken = default)
    {
        return _catalog.GetAlbumAsync(id, cancellationToken);
    }

    /// <summary>分页获取专辑歌曲。开启探测时不可播放的歌曲已被剔除。</summary>
    /// <param name="id">专辑标识。</param>
    /// <param name="query">歌曲列表请求。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>歌曲分页结果。</returns>
    public Task<SongPage> GetAlbumSongsAsync(
        AlbumId id,
        SongListQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return _catalog.GetAlbumSongsAsync(id, query, cancellationToken);
    }
}
