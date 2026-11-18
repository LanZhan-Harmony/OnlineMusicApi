using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQMusicuSearchParamDto
{
    [JsonPropertyName("num_per_page")]
    public int NumPerPage { get; set; }

    [JsonPropertyName("page_num")]
    public int PageNum { get; set; }

    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("search_type")]
    public int SearchType { get; set; }
}
