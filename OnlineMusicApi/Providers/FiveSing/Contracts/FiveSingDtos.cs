using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingSongPageDto
{
    [JsonPropertyName("isEnd")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("data")]
    public List<FiveSingSongDto>? Data { get; set; }
}

internal sealed class FiveSingAlbumPageDto
{
    [JsonPropertyName("isEnd")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("data")]
    public List<FiveSingAlbumDto>? Data { get; set; }
}

internal sealed class FiveSingArtistPageDto
{
    [JsonPropertyName("isEnd")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("data")]
    public List<FiveSingArtistDto>? Data { get; set; }
}

internal sealed class FiveSingSongDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("singerId")]
    public string? SingerId { get; set; }

    [JsonPropertyName("album")]
    public string? Album { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("typeName")]
    public string? TypeName { get; set; }

    [JsonPropertyName("typeEname")]
    public string? TypeEname { get; set; }
}

internal sealed class FiveSingAlbumDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("artwork")]
    public string? Artwork { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }
}

internal sealed class FiveSingArtistDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("fans")]
    public string? Fans { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("worksNum")]
    public string? WorksNum { get; set; }
}

internal sealed class FiveSingAlbumSongsDto
{
    [JsonPropertyName("musicList")]
    public List<FiveSingSongDto>? MusicList { get; set; }
}

internal sealed class FiveSingMediaDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

internal sealed class FiveSingLyricDto
{
    [JsonPropertyName("rawLrc")]
    public string? RawLrc { get; set; }
}