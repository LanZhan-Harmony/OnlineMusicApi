namespace OnlineMusicApi;

public sealed partial class OnlineMusicApiClient
{
    /// <summary>获取歌词。优先返回逐字格式，取不到回退普通 LRC。</summary>
    /// <param name="id">歌曲标识。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>歌词文本与格式。</returns>
    public Task<Lyric> GetLyricAsync(SongId id, CancellationToken cancellationToken = default)
    {
        return _playback.GetLyricAsync(id, cancellationToken);
    }
}
