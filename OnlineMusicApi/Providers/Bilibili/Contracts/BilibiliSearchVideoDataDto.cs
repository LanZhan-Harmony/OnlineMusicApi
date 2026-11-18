using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliSearchVideoDataDto
{
    [JsonPropertyName("result")]
    public List<BilibiliMediaDto>? Result { get; set; }

    [JsonPropertyName("numResults")]
    public int NumResults { get; set; }
}
