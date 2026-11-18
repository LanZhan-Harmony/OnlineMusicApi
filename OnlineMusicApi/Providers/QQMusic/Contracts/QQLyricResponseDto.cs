using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQLyricResponseDto
{
    [JsonPropertyName("lyric")]
    public string? Lyric { get; set; }

    [JsonPropertyName("trans")]
    public string? Trans { get; set; }
}
