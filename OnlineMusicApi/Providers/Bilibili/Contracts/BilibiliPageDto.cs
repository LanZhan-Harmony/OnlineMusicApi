using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliPageDto
{
    [JsonPropertyName("cid")]
    public long Cid { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("part")]
    public string? Part { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }
}
