using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQSingerBasicInfoDto
{
    [JsonPropertyName("singer_mid")]
    public string? SingerMid { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("singer_id")]
    public long SingerId { get; set; }
}
