using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliAudioDto
{
    [JsonPropertyName("baseUrl")]
    public string? BaseUrl { get; set; }

    [JsonPropertyName("base_url")]
    public string? BaseUrlLower { get; set; }

    [JsonPropertyName("bandwidth")]
    public int Bandwidth { get; set; }
}
