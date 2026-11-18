using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoMediaDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
