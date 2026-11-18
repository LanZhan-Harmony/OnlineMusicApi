using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSingerPageDto
{
    [JsonPropertyName("singerDetail")]
    public QQSingerDetailDto? SingerDetail { get; set; }
}
