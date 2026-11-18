using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQAlbumSongListDataDto
{
    [JsonPropertyName("songList")]
    public List<QQAlbumSongEntryDto>? SongList { get; set; }
}
