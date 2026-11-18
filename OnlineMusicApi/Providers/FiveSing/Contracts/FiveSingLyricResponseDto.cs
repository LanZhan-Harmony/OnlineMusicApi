using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingLyricResponseDto
{
    [JsonPropertyName("txt")]
    public string? Text { get; set; }

    [JsonPropertyName("lrc")]
    public string? Lrc { get; set; }
}
