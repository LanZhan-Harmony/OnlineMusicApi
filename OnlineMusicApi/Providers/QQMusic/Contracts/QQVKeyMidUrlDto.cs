using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQVKeyMidUrlDto
{
    [JsonPropertyName("purl")]
    public string? Purl { get; set; }
}
