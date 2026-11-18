using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliNavDto
{
    [JsonPropertyName("img")]
    public string? Img { get; set; }

    [JsonPropertyName("sub")]
    public string? Sub { get; set; }
}
