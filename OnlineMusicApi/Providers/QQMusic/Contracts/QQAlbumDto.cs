using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQAlbumDto
{
    [JsonPropertyName("albumid")]
    public long AlbumId { get; set; }

    [JsonPropertyName("albumID")]
    public long AlbumIdUpper { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("album_mid")]
    public string? AlbumMid { get; set; }

    [JsonPropertyName("albumMID")]
    public string? AlbumMidUpper { get; set; }

    [JsonPropertyName("albummid")]
    public string? AlbumMidLower { get; set; }

    [JsonPropertyName("album_name")]
    public string? AlbumName { get; set; }

    [JsonPropertyName("albumName")]
    public string? AlbumNameUpper { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("albumPic")]
    public string? AlbumPic { get; set; }

    [JsonPropertyName("album_pic")]
    public string? AlbumPicLower { get; set; }

    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("pub_time")]
    public string? PubTime { get; set; }

    [JsonPropertyName("publicTime")]
    public string? PublicTime { get; set; }

    [JsonPropertyName("time_public")]
    public string? TimePublic { get; set; }

    [JsonPropertyName("desc")]
    public string? Desc { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("singer_name")]
    public string? SingerName { get; set; }

    [JsonPropertyName("singerName")]
    public string? SingerNameUpper { get; set; }

    [JsonPropertyName("singer_mid")]
    public string? SingerMid { get; set; }

    [JsonPropertyName("singerMID")]
    public string? SingerMidUpper { get; set; }

    [JsonPropertyName("singer_id")]
    public long SingerId { get; set; }

    [JsonPropertyName("singers")]
    public List<QQAlbumSingerDto>? Singers { get; set; }

    [JsonPropertyName("song_count")]
    public int? SongCount { get; set; }
}
