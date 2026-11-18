using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliUserSearchDataDto
{
    [JsonPropertyName("result")]
    public List<BilibiliArtistDto>? Result { get; set; }

    [JsonPropertyName("numResults")]
    public int NumResults { get; set; }
}
