using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudSearchArtistsResult
{
    [JsonPropertyName("artistCount")]
    public int ArtistCount { get; set; }

    [JsonPropertyName("artists")]
    public List<CloudSearchArtistDto>? Artists { get; set; }
}
