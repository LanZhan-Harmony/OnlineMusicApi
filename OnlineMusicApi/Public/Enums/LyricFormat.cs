namespace OnlineMusicApi;

/// <summary>歌词文本格式。宿主据此选择解析器。</summary>
public enum LyricFormat
{
    /// <summary>未知格式或无歌词。</summary>
    Unknown = 0,

    /// <summary>标准 LRC（行级）。</summary>
    Lrc = 1,

    /// <summary>网易云逐字歌词。</summary>
    Yrc = 2,

    /// <summary>QQ 音乐逐字歌词。</summary>
    Qrc = 3,

    /// <summary>酷我逐字歌词。</summary>
    Krc = 4,
}
