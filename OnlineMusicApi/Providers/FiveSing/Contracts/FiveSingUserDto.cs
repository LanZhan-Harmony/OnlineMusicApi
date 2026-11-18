using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingUserDto
{
    [JsonPropertyName("ID")]
    public long Id { get; set; }

    [JsonPropertyName("NN")]
    public string? Name { get; set; }

    [JsonPropertyName("I")]
    public string? Avatar { get; set; }
}
