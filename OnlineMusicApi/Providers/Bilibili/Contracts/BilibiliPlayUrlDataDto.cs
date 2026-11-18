using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliPlayUrlDataDto
{
    [JsonPropertyName("dash")]
    public BilibiliDashDto? Dash { get; set; }

    [JsonPropertyName("durl")]
    public List<BilibiliDurlDto>? Durl { get; set; }
}
