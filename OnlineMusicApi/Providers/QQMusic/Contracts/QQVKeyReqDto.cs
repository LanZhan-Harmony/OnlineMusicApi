using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQVKeyReqDto
{
    [JsonPropertyName("data")]
    public QQVKeyDataDto? Data { get; set; }
}
