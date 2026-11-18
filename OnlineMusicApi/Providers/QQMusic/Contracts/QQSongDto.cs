using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSongDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("mid")]
    public string? Mid { get; set; }

    [JsonPropertyName("songmid")]
    public string? SongMid { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("songname")]
    public string? SongName { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("singer")]
    public List<QQSingerDto>? Singers { get; set; }

    [JsonPropertyName("album")]
    public QQAlbumRefDto? Album { get; set; }

    [JsonPropertyName("albumid")]
    public long AlbumId { get; set; }

    [JsonPropertyName("albummid")]
    public string? AlbumMid { get; set; }

    [JsonPropertyName("albumname")]
    public string? AlbumName { get; set; }

    [JsonPropertyName("interval")]
    public int Interval { get; set; }

    [JsonPropertyName("time_public")]
    public string? TimePublic { get; set; }
}
