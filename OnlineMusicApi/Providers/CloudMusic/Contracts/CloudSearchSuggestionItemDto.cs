using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudSearchSuggestionItemDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
