using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudSongUrlItem
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("br")]
    public int BitRate { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
