using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQAlbumSongListRequestDto
{
    [JsonPropertyName("comm")]
    public QQCommRequestDto Comm { get; set; } = new();

    [JsonPropertyName("albumSonglist")]
    public QQAlbumSongListRequestItemDto AlbumSonglist { get; set; } = new();
}
