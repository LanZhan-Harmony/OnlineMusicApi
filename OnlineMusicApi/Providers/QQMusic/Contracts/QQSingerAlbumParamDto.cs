using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSingerAlbumParamDto
{
    [JsonPropertyName("singermid")]
    public string SingerMid { get; set; } = string.Empty;

    [JsonPropertyName("order")]
    public string Order { get; set; } = "time";

    [JsonPropertyName("begin")]
    public int Begin { get; set; }

    [JsonPropertyName("num")]
    public int Num { get; set; }

    [JsonPropertyName("exstatus")]
    public int ExStatus { get; set; }
}
