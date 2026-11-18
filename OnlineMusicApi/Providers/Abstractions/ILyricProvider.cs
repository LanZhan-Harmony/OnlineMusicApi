namespace OnlineMusicApi.Internal.Providers.Abstractions;

internal interface ILyricProvider : IMusicProvider
{
    Task<Lyric> GetLyricAsync(SongId id, CancellationToken cancellationToken);
}
