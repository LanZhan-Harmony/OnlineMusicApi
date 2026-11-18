using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoPlaylistDto
{
    [JsonPropertyName("playlistid")]
    public string? PlaylistId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("nickname")]
    public string? NickName { get; set; }

    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("hts_pic")]
    public string? HtsPic { get; set; }

    [JsonPropertyName("playcnt")]
    public string? PlayCount { get; set; }

    [JsonPropertyName("intro")]
    public string? Intro { get; set; }

    [JsonPropertyName("songnum")]
    public string? SongNum { get; set; }

    [JsonPropertyName("tags")]
    public string? Tags { get; set; }
}
