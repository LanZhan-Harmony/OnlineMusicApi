using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSearchSongWrapperDto
{
    [JsonPropertyName("list")]
    public List<QQSongDto>? List { get; set; }

    [JsonPropertyName("totalnum")]
    public int TotalNum { get; set; }
}
