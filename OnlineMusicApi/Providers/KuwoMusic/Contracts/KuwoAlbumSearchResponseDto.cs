using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoAlbumSearchResponseDto
{
    [JsonPropertyName("albumlist")]
    public List<KuwoAlbumDto>? AlbumList { get; set; }

    [JsonPropertyName("PN")]
    public string? PageNumber { get; set; }

    [JsonPropertyName("RN")]
    public string? PageSize { get; set; }

    [JsonPropertyName("TOTAL")]
    public string? Total { get; set; }
}
