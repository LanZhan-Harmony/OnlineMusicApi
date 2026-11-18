using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudSearchSuggestResult
{
    [JsonPropertyName("songs")]
    public List<CloudSearchSuggestionItemDto>? Songs { get; set; }

    [JsonPropertyName("albums")]
    public List<CloudSearchSuggestionItemDto>? Albums { get; set; }

    [JsonPropertyName("artists")]
    public List<CloudSearchSuggestionItemDto>? Artists { get; set; }

    [JsonPropertyName("playlists")]
    public List<CloudSearchSuggestionItemDto>? Playlists { get; set; }
}
