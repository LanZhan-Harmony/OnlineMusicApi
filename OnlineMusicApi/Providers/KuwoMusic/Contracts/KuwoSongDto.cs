using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoSongDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("artwork")]
    public string? Artwork { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("album")]
    public string? Album { get; set; }

    [JsonPropertyName("albumId")]
    public string? AlbumId { get; set; }

    [JsonPropertyName("artistId")]
    public string? ArtistId { get; set; }

    [JsonPropertyName("formats")]
    public string? Formats { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }
}
