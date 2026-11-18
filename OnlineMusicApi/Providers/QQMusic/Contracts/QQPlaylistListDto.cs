using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQPlaylistListDto
{
    [JsonPropertyName("list")]
    public List<QQPlaylistDto>? List { get; set; }
}
