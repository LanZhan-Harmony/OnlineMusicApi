using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliMediaPageDto
{
    [JsonPropertyName("isEnd")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("data")]
    public List<BilibiliMediaDto>? Data { get; set; }
}

internal sealed class BilibiliArtistPageDto
{
    [JsonPropertyName("isEnd")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("data")]
    public List<BilibiliArtistDto>? Data { get; set; }
}

internal sealed class BilibiliMediaDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("aid")]
    public string? Aid { get; set; }

    [JsonPropertyName("bvid")]
    public string? Bvid { get; set; }

    [JsonPropertyName("cid")]
    public string? Cid { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("alias")]
    public string? Alias { get; set; }

    [JsonPropertyName("album")]
    public string? Album { get; set; }

    [JsonPropertyName("artwork")]
    public string? Artwork { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }
}

internal sealed class BilibiliArtistDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("fans")]
    public string? Fans { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("worksNum")]
    public string? WorksNum { get; set; }
}

internal sealed class BilibiliAlbumSongsDto
{
    [JsonPropertyName("musicList")]
    public List<BilibiliMediaDto>? MusicList { get; set; }
}

internal sealed class BilibiliMediaSourceDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; set; }
}

internal sealed class BilibiliUserCardDto
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public BilibiliUserCardDataDto? Data { get; set; }
}

internal sealed class BilibiliUserCardDataDto
{
    [JsonPropertyName("card")]
    public BilibiliUserCardInfoDto? Card { get; set; }
}

internal sealed class BilibiliUserCardInfoDto
{
    [JsonPropertyName("mid")]
    public string? Mid { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("face")]
    public string? Face { get; set; }

    [JsonPropertyName("sign")]
    public string? Sign { get; set; }

    [JsonPropertyName("fans")]
    public int? Fans { get; set; }

    [JsonPropertyName("attention")]
    public int? Attention { get; set; }
}