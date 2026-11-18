using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQMusicuSearchMetaDto
{
    [JsonPropertyName("sum")]
    public int Sum { get; set; }
}
