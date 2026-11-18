using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudSongTrackDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("al")]
    public CloudSongAlbumDto? Album { get; set; }

    [JsonPropertyName("ar")]
    public List<CloudSearchArtistReferenceDto>? Artists { get; set; }

    [JsonPropertyName("dt")]
    public long Duration { get; set; }
}
