using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQPlaylistResponseDto
{
    [JsonPropertyName("cdlist")]
    public List<QQCdlistDto>? Cdlist { get; set; }
}
