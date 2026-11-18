using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingSongDto
{
    [JsonPropertyName("songId")]
    public long SongId { get; set; }

    [JsonPropertyName("ID")]
    public long AlbumSongId { get; set; }

    [JsonPropertyName("songName")]
    public string? SongName { get; set; }

    [JsonPropertyName("SN")]
    public string? SequenceName { get; set; }

    [JsonPropertyName("singer")]
    public string? Singer { get; set; }

    [JsonPropertyName("singerId")]
    public long SingerId { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("typeName")]
    public string? TypeName { get; set; }

    [JsonPropertyName("typeEname")]
    public string? TypeEname { get; set; }

    [JsonPropertyName("SK")]
    public string? Sk { get; set; }

    [JsonPropertyName("user")]
    public FiveSingUserDto? User { get; set; }
}
