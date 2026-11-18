using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudArtistDescriptionResponse
{
    [JsonPropertyName("briefDesc")]
    public string? BriefDescription { get; set; }
}
