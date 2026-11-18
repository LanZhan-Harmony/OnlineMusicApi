namespace OnlineMusicApi;

/// <summary>搜索联想项的类型。</summary>
public enum SearchSuggestionKind
{
    /// <summary>无法归类。</summary>
    Unknown = 0,

    /// <summary>歌曲。</summary>
    Song = 1,

    /// <summary>专辑。</summary>
    Album = 2,

    /// <summary>艺术家。</summary>
    Artist = 3,

    /// <summary>歌单。</summary>
    Playlist = 4,
}
