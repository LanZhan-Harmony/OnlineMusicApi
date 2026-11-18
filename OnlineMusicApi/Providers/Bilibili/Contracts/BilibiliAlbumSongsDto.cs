using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliAlbumSongsDto
{
    [JsonPropertyName("musicList")]
    public List<BilibiliMediaDto>? MusicList { get; set; }
}
