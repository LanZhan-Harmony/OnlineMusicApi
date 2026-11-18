using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoPlaylistResponseDto
{
    [JsonPropertyName("musicList")]
    public List<KuwoSongDto>? MusicList { get; set; }

    [JsonPropertyName("musiclist")]
    public List<KuwoSongDto>? MusicListLower { get; set; }

    [JsonPropertyName("total")]
    public string? Total { get; set; }
}
