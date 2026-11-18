using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSongPageDto
{
    [JsonPropertyName("isEnd")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("data")]
    public List<QQSongDto>? Data { get; set; }
}

internal sealed class QQAlbumPageDto
{
    [JsonPropertyName("isEnd")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("data")]
    public List<QQAlbumDto>? Data { get; set; }
}

internal sealed class QQArtistPageDto
{
    [JsonPropertyName("isEnd")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("data")]
    public List<QQArtistDto>? Data { get; set; }
}

internal sealed class QQPlaylistPageDto
{
    [JsonPropertyName("isEnd")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("data")]
    public List<QQPlaylistDto>? Data { get; set; }
}

internal sealed class QQSongDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("songmid")]
    public string? SongMid { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("artwork")]
    public string? Artwork { get; set; }

    [JsonPropertyName("album")]
    public string? Album { get; set; }

    [JsonPropertyName("albumid")]
    public string? AlbumId { get; set; }

    [JsonPropertyName("albummid")]
    public string? AlbumMid { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }

    [JsonPropertyName("singers")]
    public List<QQSingerDto>? Singers { get; set; }
}

internal sealed class QQSingerDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("mid")]
    public string? Mid { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

internal sealed class QQAlbumDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("albumMID")]
    public string? AlbumMid { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("artwork")]
    public string? Artwork { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("singerID")]
    public string? SingerId { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("singerMID")]
    public string? SingerMid { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

internal sealed class QQArtistDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("singerMID")]
    public string? SingerMid { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("worksNum")]
    public string? WorksNum { get; set; }
}

internal sealed class QQPlaylistDto
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("createAt")]
    public string? CreateAt { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("playCount")]
    public string? PlayCount { get; set; }

    [JsonPropertyName("worksNums")]
    public string? WorksNums { get; set; }

    [JsonPropertyName("artwork")]
    public string? Artwork { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }
}

internal sealed class QQAlbumSongsDto
{
    [JsonPropertyName("musicList")]
    public List<QQSongDto>? MusicList { get; set; }
}

internal sealed class QQMediaDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

internal sealed class QQLyricDto
{
    [JsonPropertyName("rawLrc")]
    public string? RawLrc { get; set; }

    [JsonPropertyName("translation")]
    public string? Translation { get; set; }
}