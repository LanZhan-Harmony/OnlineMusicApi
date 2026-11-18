using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliFingerDataDto
{
    [JsonPropertyName("b_3")]
    public string? Buvid3 { get; set; }

    [JsonPropertyName("b_4")]
    public string? Buvid4 { get; set; }
}
