using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQLyricDto
{
    [JsonPropertyName("rawLrc")]
    public string? RawLrc { get; set; }

    [JsonPropertyName("translation")]
    public string? Translation { get; set; }
}
