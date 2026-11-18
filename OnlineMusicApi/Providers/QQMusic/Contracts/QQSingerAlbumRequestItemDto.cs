using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSingerAlbumRequestItemDto
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = "get_singer_album";

    [JsonPropertyName("param")]
    public QQSingerAlbumParamDto Param { get; set; } = new();

    [JsonPropertyName("module")]
    public string Module { get; set; } = "music.web_singer_info_svr";
}
