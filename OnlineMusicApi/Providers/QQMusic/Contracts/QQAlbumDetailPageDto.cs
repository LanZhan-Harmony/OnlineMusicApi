using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQAlbumDetailPageDto
{
    [JsonPropertyName("detail")]
    public QQAlbumDetailDto? Detail { get; set; }

    [JsonPropertyName("songList")]
    public List<QQAlbumSongListDto>? SongList { get; set; }
}
