using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSearchSongResponseDto
{
    [JsonPropertyName("data")]
    public QQSearchSongDataDto? Data { get; set; }
}
