using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudSearchSongsResult
{
    [JsonPropertyName("songCount")]
    public int SongCount { get; set; }

    [JsonPropertyName("songs")]
    public List<CloudSearchSongDto>? Songs { get; set; }
}
