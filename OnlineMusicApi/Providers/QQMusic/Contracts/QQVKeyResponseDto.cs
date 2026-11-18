using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQVKeyResponseDto
{
    [JsonPropertyName("req_0")]
    public QQVKeyReqDto? Req0 { get; set; }
}
