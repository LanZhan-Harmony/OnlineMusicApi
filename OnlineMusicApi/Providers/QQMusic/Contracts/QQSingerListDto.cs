using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSingerListDto
{
    [JsonPropertyName("list")]
    public List<QQArtistDto>? List { get; set; }
}
