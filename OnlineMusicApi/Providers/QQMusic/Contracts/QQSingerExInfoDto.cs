using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

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
