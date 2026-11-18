using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSingerAlbumContainerDto
{
    [JsonPropertyName("data")]
    public QQSingerAlbumDataDto? Data { get; set; }
}
