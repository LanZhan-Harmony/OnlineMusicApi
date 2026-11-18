using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudPlaylistDetailDto
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("trackIds")]
    public List<CloudPlaylistTrackIdDto>? TrackIds { get; set; }
}
