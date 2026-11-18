using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQMediaDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
