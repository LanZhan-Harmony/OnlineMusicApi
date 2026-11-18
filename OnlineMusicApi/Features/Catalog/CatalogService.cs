using OnlineMusicApi.Internal.Features.Shared;
using OnlineMusicApi.Internal.Providers.Abstractions;
using OnlineMusicApi.Internal.Providers.Shared;

namespace OnlineMusicApi.Internal.Features.Catalog;

internal sealed class CatalogService(ProviderRouter router)
{
    public Task<Album> GetAlbumAsync(AlbumId id, CancellationToken cancellationToken)
    {
        IAlbumProvider provider = router.Require<IAlbumProvider>(
            id.Source,
            ProviderCapability.AlbumDetail,
            nameof(GetAlbumAsync)
        );

        return provider.GetAlbumAsync(id, cancellationToken);
    }

    public async Task<SongPage> GetAlbumSongsAsync(
        AlbumId id,
        SongListQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        IAlbumProvider provider = router.Require<IAlbumProvider>(
            id.Source,
            ProviderCapability.AlbumSongs,
            nameof(GetAlbumSongsAsync)
        );

        SongPage page = await provider
            .GetAlbumSongsAsync(id, query, cancellationToken)
            .ConfigureAwait(false);

        PageValidator.EnsureSameSource(page, id.Source);
        return page;
    }

    public Task<Artist> GetArtistAsync(ArtistId id, CancellationToken cancellationToken)
    {
        IArtistProvider provider = router.Require<IArtistProvider>(
            id.Source,
            ProviderCapability.ArtistDetail,
            nameof(GetArtistAsync)
        );

        return provider.GetArtistAsync(id, cancellationToken);
    }

    public async Task<AlbumPage> GetArtistAlbumsAsync(
        ArtistId id,
        PageQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        IArtistProvider provider = router.Require<IArtistProvider>(
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

    public async Task<SongPage> GetPlaylistSongsAsync(
        PlaylistId id,
        SongListQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        IPlaylistProvider provider = router.Require<IPlaylistProvider>(
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
