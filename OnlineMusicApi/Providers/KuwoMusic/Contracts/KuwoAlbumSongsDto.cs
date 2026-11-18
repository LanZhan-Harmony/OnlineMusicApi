using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.KuwoMusic;

internal sealed class KuwoAlbumSongsDto
{
    [JsonPropertyName("musicList")]
    public List<KuwoSongDto>? MusicList { get; set; }
}
