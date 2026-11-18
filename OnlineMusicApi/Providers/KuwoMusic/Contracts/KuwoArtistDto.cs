using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoArtistDto
{
    [JsonPropertyName("ARTISTID")]
    public string? ArtistId { get; set; }

    [JsonPropertyName("ARTIST")]
    public string? Artist { get; set; }

    [JsonPropertyName("PICPATH")]
    public string? PicPath { get; set; }

    [JsonPropertyName("hts_PICPATH")]
    public string? HtsPicPath { get; set; }

    [JsonPropertyName("desc")]
    public string? Description { get; set; }

    [JsonPropertyName("SONGNUM")]
    public string? SongNum { get; set; }

    [JsonPropertyName("ALBUMNUM")]
    public string? AlbumNum { get; set; }

    [JsonPropertyName("MVNUM")]
    public string? MvNum { get; set; }

    [JsonPropertyName("COUNTRY")]
    public string? Country { get; set; }
}
