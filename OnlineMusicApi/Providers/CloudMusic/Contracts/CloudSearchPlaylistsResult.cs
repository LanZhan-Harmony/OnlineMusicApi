using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudSearchPlaylistsResult
{
    [JsonPropertyName("playlistCount")]
    public int PlaylistCount { get; set; }

    [JsonPropertyName("playlists")]
    public List<CloudSearchPlaylistDto>? Playlists { get; set; }
}
