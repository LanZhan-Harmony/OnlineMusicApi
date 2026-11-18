using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliSpaceListDto
{
    [JsonPropertyName("vlist")]
    public List<BilibiliMediaDto>? VList { get; set; }
}
