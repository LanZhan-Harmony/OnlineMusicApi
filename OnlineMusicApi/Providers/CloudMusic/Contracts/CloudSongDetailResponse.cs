using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudSongDetailResponse
{
    [JsonPropertyName("songs")]
    public List<CloudSongTrackDto>? Songs { get; set; }
}
