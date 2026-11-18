using OnlineMusicApi.Internal.Providers.Abstractions;
using OnlineMusicApi.Internal.Providers.Shared;

namespace OnlineMusicApi;

public sealed partial class OnlineMusicApiClient
{
    /// <summary>根据 ID 获取艺术家信息。</summary>
    /// <param name="id">艺术家标识。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>艺术家信息。</returns>
    public Task<Artist> GetArtistAsync(ArtistId id, CancellationToken cancellationToken = default)
    {
        IArtistProvider provider = Require<IArtistProvider>(
            id.Source,
            ProviderCapability.ArtistDetail,
            nameof(GetArtistAsync)
        );

        return provider.GetArtistAsync(id, cancellationToken);
    }

    /// <summary>分页获取艺术家拥有的专辑。</summary>
    /// <param name="id">艺术家标识。</param>
    /// <param name="query">分页请求。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>专辑分页结果。</returns>
    public async Task<AlbumPage> GetArtistAlbumsAsync(
        ArtistId id,
        PageQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        IArtistProvider provider = Require<IArtistProvider>(
            id.Source,
            ProviderCapability.ArtistAlbums,
            nameof(GetArtistAlbumsAsync)
        );

        AlbumPage page = await provider
            .GetArtistAlbumsAsync(id, query, cancellationToken)
            .ConfigureAwait(false);
        PageValidator.EnsureSameSource(page, id.Source);
        return page;
    }
}
