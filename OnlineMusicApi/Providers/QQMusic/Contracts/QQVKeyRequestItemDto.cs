using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQVKeyRequestItemDto
{
    [JsonPropertyName("module")]
    public string Module { get; set; } = "vkey.GetVkeyServer";

    [JsonPropertyName("method")]
    public string Method { get; set; } = "CgiGetVkey";

    [JsonPropertyName("param")]
    public QQVKeyParamDto Param { get; set; } = new();
}
