using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingAlbumSongsResponseDto
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("data")]
    public List<FiveSingSongDto>? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
