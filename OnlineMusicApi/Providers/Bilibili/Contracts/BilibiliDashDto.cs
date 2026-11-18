using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliDashDto
{
    [JsonPropertyName("audio")]
    public List<BilibiliAudioDto>? Audio { get; set; }
}
