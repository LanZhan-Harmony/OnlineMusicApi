using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudSearchAlbumsResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("result")]
    public CloudSearchAlbumsResult? Result { get; set; }
}
