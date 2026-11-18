using OnlineMusicApi.Internal.Features.Shared;
using OnlineMusicApi.Internal.Providers.Abstractions;

namespace OnlineMusicApi.Internal.Features.Playback;

internal sealed class PlaybackService(ProviderRouter router)
{
    public async Task<MediaSource> ResolveMediaAsync(
        SongId id,
        AudioQuality quality,
        CancellationToken cancellationToken
    )
    {
        IPlaybackProvider provider = router.Require<IPlaybackProvider>(
            id.Source,
            ProviderCapability.Playback,
            nameof(ResolveMediaAsync)
        );

        MediaSource? media = await provider
            .ResolveMediaAsync(id, quality, cancellationToken)
            .ConfigureAwait(false);

        return media
            ?? throw new OnlineMusicApiMediaException(
                "该歌曲不可播放。",
                nameof(ResolveMediaAsync),
                id.Source
            );
    }

    public Task<Lyric> GetLyricAsync(SongId id, CancellationToken cancellationToken)
    {
        ILyricProvider provider = router.Require<ILyricProvider>(
            id.Source,
            ProviderCapability.Lyric,
            nameof(GetLyricAsync)
        );

        return provider.GetLyricAsync(id, cancellationToken);
    }
}
