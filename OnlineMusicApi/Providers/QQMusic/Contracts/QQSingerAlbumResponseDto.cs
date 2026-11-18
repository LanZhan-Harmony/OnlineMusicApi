using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSingerAlbumResponseDto
{
    [JsonPropertyName("singerAlbum")]
    public QQSingerAlbumContainerDto? SingerAlbum { get; set; }
}
