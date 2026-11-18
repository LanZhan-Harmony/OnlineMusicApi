using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSingerPageDto
{
    [JsonPropertyName("singerDetail")]
    public QQSingerDetailDto? SingerDetail { get; set; }
}

internal sealed class QQSingerDetailDto
{
    [JsonPropertyName("basic_info")]
    public QQSingerBasicInfoDto? BasicInfo { get; set; }

    [JsonPropertyName("ex_info")]
    public QQSingerExInfoDto? ExInfo { get; set; }

    [JsonPropertyName("pic")]
    public QQSingerPicDto? Pic { get; set; }

    [JsonPropertyName("shortdesc")]
    public string? ShortDesc { get; set; }

    [JsonPropertyName("albumTotalNum")]
    public int? AlbumTotalNum { get; set; }

    [JsonPropertyName("songTotalNum")]
    public int? SongTotalNum { get; set; }
}

internal sealed class QQSingerBasicInfoDto
{
    [JsonPropertyName("singer_mid")]
    public string? SingerMid { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("singer_id")]
    public long SingerId { get; set; }
}

internal sealed class QQSingerExInfoDto
{
    [JsonPropertyName("desc")]
    public string? Desc { get; set; }

    [JsonPropertyName("albumTotalNum")]
    public int? AlbumTotalNum { get; set; }

    [JsonPropertyName("songTotalNum")]
    public int? SongTotalNum { get; set; }

    [JsonPropertyName("concernNum")]
    public int? ConcernNum { get; set; }
}

internal sealed class QQSingerPicDto
{
    [JsonPropertyName("pic")]
    public string? Pic { get; set; }
}

internal sealed class QQAlbumDetailPageDto
{
    [JsonPropertyName("detail")]
    public QQAlbumDetailDto? Detail { get; set; }

    [JsonPropertyName("songList")]
    public List<QQAlbumSongListDto>? SongList { get; set; }
}

internal sealed class QQAlbumDetailDto
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("picurl")]
    public string? PicUrl { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("albumMid")]
    public string? AlbumMid { get; set; }

    [JsonPropertyName("singer")]
    public List<QQAlbumSingerDto>? Singer { get; set; }

    [JsonPropertyName("albumName")]
    public string? AlbumName { get; set; }

    [JsonPropertyName("ctime")]
    public string? CTime { get; set; }

    [JsonPropertyName("desc")]
    public string? Desc { get; set; }
}

internal sealed class QQAlbumSingerDto
{
    [JsonPropertyName("mid")]
    public string? Mid { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("singerID")]
    public long SingerId { get; set; }
}

internal sealed class QQAlbumSongListDto;