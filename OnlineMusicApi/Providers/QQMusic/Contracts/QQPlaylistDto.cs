using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQPlaylistDto
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("createAt")]
    public string? CreateAt { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("playCount")]
    public string? PlayCount { get; set; }

    [JsonPropertyName("worksNums")]
    public string? WorksNums { get; set; }

    [JsonPropertyName("artwork")]
    public string? Artwork { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }
}
