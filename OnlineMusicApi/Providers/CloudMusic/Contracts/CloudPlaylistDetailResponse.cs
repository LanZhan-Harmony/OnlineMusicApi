using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudPlaylistDetailResponse
{
    [JsonPropertyName("playlist")]
    public CloudPlaylistDetailDto? Playlist { get; set; }
}
