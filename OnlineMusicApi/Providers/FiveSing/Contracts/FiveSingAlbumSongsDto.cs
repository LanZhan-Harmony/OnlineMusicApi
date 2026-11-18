using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingAlbumSongsDto
{
    [JsonPropertyName("musicList")]
    public List<FiveSingSongDto>? MusicList { get; set; }
}
