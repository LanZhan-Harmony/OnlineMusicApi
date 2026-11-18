using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoLyricResponseDto
{
    [JsonPropertyName("data")]
    public KuwoLyricDataDto? Data { get; set; }
}
