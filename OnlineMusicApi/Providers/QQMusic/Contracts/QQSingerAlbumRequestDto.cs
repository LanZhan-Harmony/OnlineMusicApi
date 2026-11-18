using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSingerAlbumRequestDto
{
    [JsonPropertyName("comm")]
    public QQCommRequestDto Comm { get; set; } = new();

    [JsonPropertyName("singerAlbum")]
    public QQSingerAlbumRequestItemDto SingerAlbum { get; set; } = new();
}
