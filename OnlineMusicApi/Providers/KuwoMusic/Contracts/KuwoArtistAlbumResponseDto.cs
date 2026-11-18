using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoArtistAlbumResponseDto
{
    [JsonPropertyName("albumlist")]
    public List<KuwoAlbumDto>? AlbumList { get; set; }

    [JsonPropertyName("pn")]
    public string? PageNumber { get; set; }

    [JsonPropertyName("total")]
    public string? Total { get; set; }
}
