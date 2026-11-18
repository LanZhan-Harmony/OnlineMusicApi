using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQCdlistDto
{
    [JsonPropertyName("songlist")]
    public List<QQSongDto>? Songlist { get; set; }
}
