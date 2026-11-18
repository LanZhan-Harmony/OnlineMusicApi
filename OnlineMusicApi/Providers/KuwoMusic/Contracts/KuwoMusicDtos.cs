using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoSongPageDto
{
    [JsonPropertyName("isEnd")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("data")]
    public List<KuwoSongDto>? Data { get; set; }
}

internal sealed class KuwoAlbumPageDto
{
    [JsonPropertyName("isEnd")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("data")]
    public List<KuwoAlbumDto>? Data { get; set; }
}

internal sealed class KuwoArtistPageDto
{
    [JsonPropertyName("isEnd")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("data")]
    public List<KuwoArtistDto>? Data { get; set; }
}

internal sealed class KuwoPlaylistPageDto
{
    [JsonPropertyName("isEnd")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("data")]
    public List<KuwoPlaylistDto>? Data { get; set; }
}

internal sealed class KuwoSongDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("artwork")]
    public string? Artwork { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("album")]
    public string? Album { get; set; }

    [JsonPropertyName("albumId")]
    public string? AlbumId { get; set; }

    [JsonPropertyName("artistId")]
    public string? ArtistId { get; set; }

    [JsonPropertyName("formats")]
    public string? Formats { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }
}

internal sealed class KuwoAlbumDto
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

    [JsonPropertyName("artistId")]
    public string? ArtistId { get; set; }
}

internal sealed class KuwoArtistDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("artistId")]
    public string? ArtistId { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("worksNum")]
    public string? WorksNum { get; set; }
}

internal sealed class KuwoPlaylistDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("artwork")]
    public string? Artwork { get; set; }

    [JsonPropertyName("playCount")]
    public string? PlayCount { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("worksNum")]
    public string? WorksNum { get; set; }
}

internal sealed class KuwoAlbumSongsDto
{
    [JsonPropertyName("musicList")]
    public List<KuwoSongDto>? MusicList { get; set; }
}

internal sealed class KuwoMediaDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

internal sealed class KuwoLyricDto
{
    [JsonPropertyName("rawLrc")]
    public string? RawLrc { get; set; }
}