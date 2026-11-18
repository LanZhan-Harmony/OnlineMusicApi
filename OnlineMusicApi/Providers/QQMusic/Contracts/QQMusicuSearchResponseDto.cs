using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQMusicuSearchResponseDto
{
    [JsonPropertyName("req_1")]
    public QQMusicuSearchDataDto? Req1 { get; set; }
}
