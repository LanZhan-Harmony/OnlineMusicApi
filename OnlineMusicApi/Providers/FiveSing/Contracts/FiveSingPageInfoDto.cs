using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingPageInfoDto
{
    [JsonPropertyName("cur")]
    public int Current { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
}
