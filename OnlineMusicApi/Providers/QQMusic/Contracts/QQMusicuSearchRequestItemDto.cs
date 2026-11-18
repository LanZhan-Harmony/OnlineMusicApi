using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQMusicuSearchRequestItemDto
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = "DoSearchForQQMusicDesktop";

    [JsonPropertyName("module")]
    public string Module { get; set; } = "music.search.SearchCgiService";

    [JsonPropertyName("param")]
    public QQMusicuSearchParamDto Param { get; set; } = new();
}
