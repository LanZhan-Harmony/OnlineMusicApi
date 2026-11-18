namespace OnlineMusicApi;

public sealed partial class OnlineMusicApiClient
{
    /// <summary>获取播放地址。源降级时会回填 <see cref="MediaSource.ActualQuality"/>。</summary>
    /// <param name="id">歌曲标识。</param>
    /// <param name="quality">期望音质。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>播放源信息。</returns>
    /// <exception cref="OnlineMusicApiMediaException">该歌曲不可播放。</exception>
    public Task<MediaSource> ResolveMediaAsync(
        SongId id,
        AudioQuality quality = AudioQuality.Highest,
        CancellationToken cancellationToken = default
    )
    {
        return _playback.ResolveMediaAsync(id, quality, cancellationToken);
    }
}
