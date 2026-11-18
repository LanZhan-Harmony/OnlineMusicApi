using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

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
