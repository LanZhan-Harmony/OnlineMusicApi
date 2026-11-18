using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSearchSongDataDto
{
    [JsonPropertyName("song")]
    public QQSearchSongWrapperDto? Song { get; set; }
}
