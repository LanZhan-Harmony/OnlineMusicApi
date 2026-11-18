using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQAlbumSongsDto
{
    [JsonPropertyName("musicList")]
    public List<QQSongDto>? MusicList { get; set; }
}
