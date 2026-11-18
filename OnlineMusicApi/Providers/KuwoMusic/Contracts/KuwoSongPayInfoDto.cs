using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoSongPayInfoDto
{
    [JsonPropertyName("listen_fragment")]
    public string? ListenFragment { get; set; }
}
