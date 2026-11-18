using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoLyricDto
{
    [JsonPropertyName("rawLrc")]
    public string? RawLrc { get; set; }
}
