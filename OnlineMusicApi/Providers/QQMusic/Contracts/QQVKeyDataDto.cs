using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQVKeyDataDto
{
    [JsonPropertyName("sip")]
    public List<string>? Sip { get; set; }

    [JsonPropertyName("midurlinfo")]
    public List<QQVKeyMidUrlDto>? MidUrlInfo { get; set; }
}
