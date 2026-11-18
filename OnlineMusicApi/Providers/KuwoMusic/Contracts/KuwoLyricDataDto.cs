using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoLyricDataDto
{
    [JsonPropertyName("lrclist")]
    public List<KuwoLyricLineDto>? LrcList { get; set; }
}
