using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudArtistAlbumResponse
{
    [JsonPropertyName("artist")]
    public CloudArtistDto? Artist { get; set; }

    [JsonPropertyName("hotAlbums")]
    public List<CloudArtistAlbumDto>? Albums { get; set; }
}
