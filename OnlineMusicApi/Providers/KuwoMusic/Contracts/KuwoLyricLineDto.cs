using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoLyricLineDto
{
    [JsonPropertyName("time")]
    public string? Time { get; set; }

    [JsonPropertyName("lineLyric")]
    public string? LineLyric { get; set; }
}
