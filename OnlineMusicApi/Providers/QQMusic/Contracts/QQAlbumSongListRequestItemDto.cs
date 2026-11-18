using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQAlbumSongListRequestItemDto
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = "GetAlbumSongList";

    [JsonPropertyName("param")]
    public QQAlbumSongListParamDto Param { get; set; } = new();

    [JsonPropertyName("module")]
    public string Module { get; set; } = "music.musichallAlbum.AlbumSongList";
}
