using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliDurlDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
