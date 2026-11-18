using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQVKeyParamDto
{
    [JsonPropertyName("guid")]
    public string Guid { get; set; } = string.Empty;

    [JsonPropertyName("songmid")]
    public string[] SongMid { get; set; } = [];

    [JsonPropertyName("songtype")]
    public int[] SongType { get; set; } = [0];

    [JsonPropertyName("uin")]
    public string Uin { get; set; } = "0";

    [JsonPropertyName("loginflag")]
    public int LoginFlag { get; set; } = 1;

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = "20";
}
