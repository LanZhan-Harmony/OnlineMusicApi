using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingSearchArtistResponseDto
{
    [JsonPropertyName("list")]
    public List<FiveSingArtistDto>? List { get; set; }

    [JsonPropertyName("pageInfo")]
    public FiveSingPageInfoDto? PageInfo { get; set; }
}
