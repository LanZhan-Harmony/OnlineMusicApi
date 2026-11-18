using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliViewDataDto
{
    [JsonPropertyName("bvid")]
    public string? Bvid { get; set; }

    [JsonPropertyName("aid")]
    public long Aid { get; set; }

    [JsonPropertyName("cid")]
    public long Cid { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("pubdate")]
    public long PubDate { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("owner")]
    public BilibiliOwnerDto? Owner { get; set; }

    [JsonPropertyName("pages")]
    public List<BilibiliPageDto>? Pages { get; set; }
}
