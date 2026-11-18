namespace OnlineMusicApi;

/// <summary>歌词。只承载解密/解压后的原始文本，不做行、字解析。</summary>
public sealed class Lyric
{
    /// <summary>所属歌曲标识。</summary>
    public required SongId Id { get; init; }

    /// <summary>歌词格式。宿主据此选择解析器。</summary>
    public LyricFormat Format { get; init; }

    /// <summary>歌词明文。无歌词时为空字符串。</summary>
    public string Text { get; init; } = string.Empty;
}
