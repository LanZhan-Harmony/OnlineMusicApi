using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQAlbumListDto
{
    [JsonPropertyName("list")]
    public List<QQAlbumDto>? List { get; set; }
}
