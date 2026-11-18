namespace OnlineMusicApi;

/// <summary>音源支持的能力。可按位组合。</summary>
[Flags]
public enum ProviderCapability
{
    /// <summary>无能力。</summary>
    None = 0,

    /// <summary>搜索歌曲。</summary>
    SearchSong = 1 << 0,

    /// <summary>搜索专辑。</summary>
    SearchAlbum = 1 << 1,

    /// <summary>搜索艺术家。</summary>
    SearchArtist = 1 << 2,

    /// <summary>搜索歌单。</summary>
    SearchPlaylist = 1 << 3,

    /// <summary>搜索联想。</summary>
    SearchSuggestion = 1 << 4,

    /// <summary>获取播放地址。</summary>
    Playback = 1 << 5,

    /// <summary>获取歌词。</summary>
    Lyric = 1 << 6,

    /// <summary>获取专辑信息。</summary>
    AlbumDetail = 1 << 7,

    /// <summary>分页获取专辑歌曲。</summary>
    AlbumSongs = 1 << 8,

    /// <summary>获取艺术家信息。</summary>
    ArtistDetail = 1 << 9,

    /// <summary>分页获取艺术家专辑。</summary>
    ArtistAlbums = 1 << 10,

    /// <summary>分页获取歌单歌曲。</summary>
    PlaylistSongs = 1 << 11,
}
