namespace OnlineMusicApi;

/// <summary>在线音乐源标识。宿主的"乐库"tab 由宿主自行映射到这些值。</summary>
public enum MusicSource
{
    /// <summary>网易云音乐。</summary>
    CloudMusic = 0,

    /// <summary>QQ 音乐。</summary>
    QQMusic = 1,

    /// <summary>酷我音乐。</summary>
    KuwoMusic = 2,

    /// <summary>哔哩哔哩。</summary>
    Bilibili = 3,

    /// <summary>5sing。</summary>
    FiveSing = 4,
}
