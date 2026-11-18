using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingLyricDto
{
    [JsonPropertyName("rawLrc")]
    public string? RawLrc { get; set; }
}
