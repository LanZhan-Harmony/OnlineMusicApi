using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoAlbumInfoResponseDto
{
    [JsonPropertyName("musiclist")]
    public List<KuwoSongDto>? MusicList { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("artistid")]
    public string? ArtistId { get; set; }

    [JsonPropertyName("pub")]
    public string? Pub { get; set; }

    [JsonPropertyName("info")]
    public string? Info { get; set; }

    [JsonPropertyName("songnum")]
    public string? SongNum { get; set; }

    [JsonPropertyName("hts_img")]
    public string? HtsImg { get; set; }

    [JsonPropertyName("img")]
    public string? Img { get; set; }
}
