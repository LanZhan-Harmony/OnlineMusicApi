using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQMusicuSearchRequestDto
{
    [JsonPropertyName("req_1")]
    public QQMusicuSearchRequestItemDto Req1 { get; set; } = new();
}
