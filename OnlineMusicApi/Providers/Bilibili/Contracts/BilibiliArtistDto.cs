using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliArtistDto
{
    [JsonPropertyName("mid")]
    public long Mid { get; set; }

    [JsonPropertyName("uname")]
    public string? Name { get; set; }

    [JsonPropertyName("upic")]
    public string? Avatar { get; set; }

    [JsonPropertyName("videos")]
    public int? Videos { get; set; }

    [JsonPropertyName("usign")]
    public string? Description { get; set; }

    [JsonPropertyName("fans")]
    public int? Fans { get; set; }
}
