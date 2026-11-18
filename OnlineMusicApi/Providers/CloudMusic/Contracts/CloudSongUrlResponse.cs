using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudSongUrlResponse
{
    [JsonPropertyName("data")]
    public List<CloudSongUrlItem>? Data { get; set; }
}
