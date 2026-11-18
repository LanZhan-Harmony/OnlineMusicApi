using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingSearchAlbumResponseDto
{
    [JsonPropertyName("list")]
    public List<FiveSingAlbumDto>? List { get; set; }

    [JsonPropertyName("pageInfo")]
    public FiveSingPageInfoDto? PageInfo { get; set; }
}
