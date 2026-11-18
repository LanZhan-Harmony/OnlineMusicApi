using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliArtistPageDto
{
    [JsonPropertyName("isEnd")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("data")]
    public List<BilibiliArtistDto>? Data { get; set; }
}
