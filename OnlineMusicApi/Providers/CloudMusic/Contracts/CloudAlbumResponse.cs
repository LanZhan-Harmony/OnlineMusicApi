using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudAlbumResponse
{
    [JsonPropertyName("album")]
    public CloudAlbumDetailDto? Album { get; set; }

    [JsonPropertyName("songs")]
    public List<CloudSongTrackDto>? Songs { get; set; }
}
