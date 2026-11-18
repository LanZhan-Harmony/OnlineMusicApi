using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQAlbumSongListContainerDto
{
    [JsonPropertyName("data")]
    public QQAlbumSongListDataDto? Data { get; set; }
}
