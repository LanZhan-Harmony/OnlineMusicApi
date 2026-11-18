namespace OnlineMusicApi;

/// <summary>分页结果。</summary>
/// <typeparam name="T">列表项类型。</typeparam>
public class Page<T>
{
    /// <summary>本页数据所属的音源。</summary>
    public MusicSource Source { get; init; }

    /// <summary>本页有效项。</summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>本页真实获取到的有效条数，恒等于 <c>Items.Count</c>。不是上游结果总数。</summary>
    public int Total { get; init; }

    /// <summary>
    /// 下一页应从哪个上游 offset 开始请求；<c>HasMore == false</c> 时为 null。
    /// 与 <see cref="Total"/> 没有算术关系，调用方禁止用 <c>Offset + Total</c> 计算下一页。
    /// </summary>
    public int? NextOffset { get; init; }

    /// <summary>上游是否还有数据。</summary>
    public bool HasMore { get; init; }

    /// <summary>非致命降级说明，例如探测剔除、音质降级、未凑满一页。</summary>
    public string? Warning { get; init; }
}

/// <summary>歌曲分页结果。</summary>
public sealed class SongPage : Page<Song>;

/// <summary>专辑分页结果。</summary>
public sealed class AlbumPage : Page<Album>;

/// <summary>艺术家分页结果。</summary>
public sealed class ArtistPage : Page<Artist>;

/// <summary>歌单分页结果。</summary>
public sealed class PlaylistPage : Page<Playlist>;
