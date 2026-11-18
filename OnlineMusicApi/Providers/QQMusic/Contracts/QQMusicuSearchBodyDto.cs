using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQMusicuSearchBodyDto
{
    [JsonPropertyName("album")]
    public QQAlbumListDto? Album { get; set; }

    [JsonPropertyName("singer")]
    public QQSingerListDto? Singer { get; set; }

    [JsonPropertyName("playlist")]
    public QQPlaylistListDto? Playlist { get; set; }

    [JsonPropertyName("song")]
    public QQSongListDto? Song { get; set; }
}
