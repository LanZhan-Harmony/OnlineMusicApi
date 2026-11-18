using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoAlbumDto
{
    [JsonPropertyName("albumid")]
    public string? AlbumId { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("album")]
    public string? Album { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("artistid")]
    public string? ArtistId { get; set; }

    [JsonPropertyName("fartist")]
    public string? FullArtist { get; set; }

    [JsonPropertyName("info")]
    public string? Info { get; set; }

    [JsonPropertyName("pub")]
    public string? Pub { get; set; }

    [JsonPropertyName("hts_img")]
    public string? HtsImg { get; set; }

    [JsonPropertyName("img")]
    public string? Img { get; set; }

    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("musiccnt")]
    public string? MusicCount { get; set; }

    [JsonPropertyName("songnum")]
    public string? SongNum { get; set; }
}
