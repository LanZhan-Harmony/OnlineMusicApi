using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQMusicuSearchDataDto
{
    [JsonPropertyName("body")]
    public QQMusicuSearchBodyDto? Body { get; set; }

    [JsonPropertyName("meta")]
    public QQMusicuSearchMetaDto? Meta { get; set; }
}
