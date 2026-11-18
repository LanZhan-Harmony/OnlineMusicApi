using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

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
