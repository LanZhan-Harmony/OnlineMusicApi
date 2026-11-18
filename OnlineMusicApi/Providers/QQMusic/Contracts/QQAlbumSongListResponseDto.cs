using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQAlbumSongListResponseDto
{
    [JsonPropertyName("albumSonglist")]
    public QQAlbumSongListContainerDto? AlbumSonglist { get; set; }
}
