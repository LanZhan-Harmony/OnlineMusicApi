using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudLyricDto
{
    [JsonPropertyName("lyric")]
    public string? Lyric { get; set; }
}
