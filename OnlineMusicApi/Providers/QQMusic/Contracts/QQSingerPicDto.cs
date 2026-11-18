using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSingerPicDto
{
    [JsonPropertyName("pic")]
    public string? Pic { get; set; }
}
