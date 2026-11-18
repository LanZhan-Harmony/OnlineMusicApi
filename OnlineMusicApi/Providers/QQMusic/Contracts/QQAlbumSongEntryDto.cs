using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQAlbumSongEntryDto
{
    [JsonPropertyName("songInfo")]
    public QQSongDto? SongInfo { get; set; }
}
