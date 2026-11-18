using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQAlbumSongListParamDto
{
    [JsonPropertyName("albumMid")]
    public string AlbumMid { get; set; } = string.Empty;

    [JsonPropertyName("albumID")]
    public int AlbumId { get; set; }

    [JsonPropertyName("begin")]
    public int Begin { get; set; }

    [JsonPropertyName("num")]
    public int Num { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }
}
