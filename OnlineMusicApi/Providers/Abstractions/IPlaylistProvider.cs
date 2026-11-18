namespace OnlineMusicApi.Internal.Providers.Abstractions;

internal interface IPlaylistProvider : IMusicProvider
{
    Task<SongPage> GetPlaylistSongsAsync(
        PlaylistId id,
        SongListQuery query,
        CancellationToken cancellationToken
    );
}
