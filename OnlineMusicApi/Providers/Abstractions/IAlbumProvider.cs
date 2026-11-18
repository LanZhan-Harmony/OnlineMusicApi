namespace OnlineMusicApi.Internal.Providers.Abstractions;

internal interface IAlbumProvider : IMusicProvider
{
    Task<Album> GetAlbumAsync(AlbumId id, CancellationToken cancellationToken);

    Task<SongPage> GetAlbumSongsAsync(
        AlbumId id,
        SongListQuery query,
        CancellationToken cancellationToken
    );
}
