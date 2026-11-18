using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudSearchAlbumsResult
{
    [JsonPropertyName("albumCount")]
    public int AlbumCount { get; set; }

    [JsonPropertyName("albums")]
    public List<CloudSearchAlbumDto>? Albums { get; set; }
}
