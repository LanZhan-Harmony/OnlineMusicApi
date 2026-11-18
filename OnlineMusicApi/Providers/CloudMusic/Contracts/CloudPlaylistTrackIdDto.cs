using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudPlaylistTrackIdDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
}
