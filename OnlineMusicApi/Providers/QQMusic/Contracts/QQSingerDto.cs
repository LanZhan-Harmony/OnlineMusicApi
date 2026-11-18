using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSingerDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("mid")]
    public string? Mid { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
