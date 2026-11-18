using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingSearchSongResponseDto
{
    [JsonPropertyName("list")]
    public List<FiveSingSongDto>? List { get; set; }

    [JsonPropertyName("pageInfo")]
    public FiveSingPageInfoDto? PageInfo { get; set; }
}
