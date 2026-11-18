namespace OnlineMusicApi.Internal.Providers.Abstractions;

internal interface IPlaybackProvider : IMusicProvider
{
    Task<MediaSource?> ResolveMediaAsync(
        SongId id,
        AudioQuality quality,
        CancellationToken cancellationToken
    );
}
