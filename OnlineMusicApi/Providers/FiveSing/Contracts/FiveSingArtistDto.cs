using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingArtistDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("nickName")]
    public string? NickName { get; set; }

    [JsonPropertyName("pictureUrl")]
    public string? PictureUrl { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("fans")]
    public int? Fans { get; set; }

    [JsonPropertyName("totalSong")]
    public int? TotalSong { get; set; }
}
