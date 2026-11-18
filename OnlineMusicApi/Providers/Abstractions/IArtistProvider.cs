namespace OnlineMusicApi.Internal.Providers.Abstractions;

internal interface IArtistProvider : IMusicProvider
{
    Task<Artist> GetArtistAsync(ArtistId id, CancellationToken cancellationToken);

    Task<AlbumPage> GetArtistAlbumsAsync(
        ArtistId id,
        PageQuery query,
        CancellationToken cancellationToken
    );
}
