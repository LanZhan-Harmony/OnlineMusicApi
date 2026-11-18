using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

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
