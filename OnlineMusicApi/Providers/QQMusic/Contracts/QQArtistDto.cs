using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQArtistDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("singerName")]
    public string? SingerName { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("singerMID")]
    public string? SingerMid { get; set; }

    [JsonPropertyName("singerPic")]
    public string? SingerPic { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("worksNum")]
    public int? WorksNum { get; set; }

    [JsonPropertyName("musicSize")]
    public int? MusicSize { get; set; }

    [JsonPropertyName("albumSize")]
    public int? AlbumSize { get; set; }
}
