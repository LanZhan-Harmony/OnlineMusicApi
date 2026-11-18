using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliSpacePageDto
{
    [JsonPropertyName("pn")]
    public int PageNumber { get; set; }

    [JsonPropertyName("ps")]
    public int PageSize { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
