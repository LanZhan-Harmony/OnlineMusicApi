using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingSongUrlResponseDto
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("data")]
    public FiveSingSongUrlDataDto? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
