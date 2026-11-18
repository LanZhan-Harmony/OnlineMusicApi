using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSongListDto
{
    [JsonPropertyName("list")]
    public List<QQSongDto>? List { get; set; }
}
