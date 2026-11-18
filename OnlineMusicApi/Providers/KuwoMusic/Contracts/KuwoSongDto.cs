using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoSongDto
{
    [JsonPropertyName("MUSICRID")]
    public string? MusicRid { get; set; }

    [JsonPropertyName("musicrid")]
    public string? MusicRidLower { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("NAME")]
    public string? Name { get; set; }

    [JsonPropertyName("SONGNAME")]
    public string? SongName { get; set; }

    [JsonPropertyName("name")]
    public string? NameLower { get; set; }

    [JsonPropertyName("ARTIST")]
    public string? Artist { get; set; }

    [JsonPropertyName("artist")]
    public string? ArtistLower { get; set; }

    [JsonPropertyName("ALBUM")]
    public string? Album { get; set; }

    [JsonPropertyName("album")]
    public string? AlbumLower { get; set; }

    [JsonPropertyName("ALBUMID")]
    public string? AlbumId { get; set; }

    [JsonPropertyName("albumid")]
    public string? AlbumIdLower { get; set; }

    [JsonPropertyName("albumId")]
    public long? AlbumIdNumber { get; set; }

    [JsonPropertyName("ARTISTID")]
    public string? ArtistId { get; set; }

    [JsonPropertyName("artistid")]
    public string? ArtistIdLower { get; set; }

    [JsonPropertyName("DURATION")]
    public string? Duration { get; set; }

    [JsonPropertyName("duration")]
    public string? DurationLower { get; set; }

    [JsonPropertyName("FORMATS")]
    public string? Formats { get; set; }

    [JsonPropertyName("formats")]
    public string? FormatsLower { get; set; }

    [JsonPropertyName("web_albumpic_short")]
    public string? ArtworkShort { get; set; }

    [JsonPropertyName("web_artistpic_short")]
    public string? ArtistPicShort { get; set; }

    [JsonPropertyName("pic120")]
    public string? Pic120 { get; set; }

    [JsonPropertyName("img")]
    public string? Img { get; set; }

    [JsonPropertyName("releasedate")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("payInfo")]
    public KuwoSongPayInfoDto? PayInfo { get; set; }
}
