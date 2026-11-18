using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQVKeyRequestDto
{
    [JsonPropertyName("req_0")]
    public QQVKeyRequestItemDto Req0 { get; set; } = new();

    [JsonPropertyName("comm")]
    public QQCommRequestDto Comm { get; set; } = new();
}
