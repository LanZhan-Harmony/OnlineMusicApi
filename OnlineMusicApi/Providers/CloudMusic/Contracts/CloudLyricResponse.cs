using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudLyricResponse
{
    [JsonPropertyName("lrc")]
    public CloudLyricDto? Lrc { get; set; }

    [JsonPropertyName("yrc")]
    public CloudLyricDto? Yrc { get; set; }
}
