using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliOwnerDto
{
    [JsonPropertyName("mid")]
    public long Mid { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("face")]
    public string? Face { get; set; }
}
