using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliMediaDto
{
    [JsonPropertyName("aid")]
    public long Aid { get; set; }

    [JsonPropertyName("bvid")]
    public string? Bvid { get; set; }

    [JsonPropertyName("cid")]
    public long? Cid { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("owner")]
    public BilibiliOwnerDto? Owner { get; set; }

    [JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [JsonPropertyName("duration")]
    public string? DurationText { get; set; }

    [JsonPropertyName("pubdate")]
    public long? PubDate { get; set; }

    [JsonPropertyName("created")]
    public long? Created { get; set; }

    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("part")]
    public string? Part { get; set; }

    [JsonPropertyName("page")]
    public int? Page { get; set; }
}
