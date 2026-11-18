using OnlineMusicApi.Internal.Providers.Abstractions;
using OnlineMusicApi.Internal.Providers.Shared;

namespace OnlineMusicApi;

public sealed partial class OnlineMusicApiClient
{
    /// <summary>分页获取歌单歌曲。开启探测时不可播放的歌曲已被剔除。</summary>
    /// <param name="id">歌单标识。</param>
    /// <param name="query">歌曲列表请求。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>歌曲分页结果。</returns>
    public async Task<SongPage> GetPlaylistSongsAsync(
        PlaylistId id,
        SongListQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        IPlaylistProvider provider = Require<IPlaylistProvider>(
            id.Source,
            ProviderCapability.PlaylistSongs,
            nameof(GetPlaylistSongsAsync)
        );

        SongPage page = await provider
            .GetPlaylistSongsAsync(id, query, cancellationToken)
            .ConfigureAwait(false);
        PageValidator.EnsureSameSource(page, id.Source);
        return page;
    }
}
