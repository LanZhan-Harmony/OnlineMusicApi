using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudSearchSongDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("album")]
    public CloudSearchAlbumReferenceDto? Album { get; set; }

    [JsonPropertyName("artists")]
    public List<CloudSearchArtistReferenceDto>? Artists { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }
}
