using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQCommRequestDto
{
    [JsonPropertyName("ct")]
    public int Ct { get; set; }

    [JsonPropertyName("cv")]
    public int Cv { get; set; }

    [JsonPropertyName("uin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Uin { get; set; }

    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Format { get; set; }
}
