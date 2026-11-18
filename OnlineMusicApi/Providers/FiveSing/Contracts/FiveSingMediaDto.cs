using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingMediaDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
