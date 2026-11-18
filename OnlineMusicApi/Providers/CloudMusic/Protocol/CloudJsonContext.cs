using System.Text.Json.Serialization;
using OnlineMusicApi.Internal.Contracts.CloudMusic;

namespace OnlineMusicApi.Internal.Providers.CloudMusic.Protocol;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(CloudSearchSongsResponse))]
[JsonSerializable(typeof(CloudSearchAlbumsResponse))]
[JsonSerializable(typeof(CloudSearchArtistsResponse))]
[JsonSerializable(typeof(CloudSearchPlaylistsResponse))]
[JsonSerializable(typeof(CloudSearchSuggestResponse))]
[JsonSerializable(typeof(CloudSongUrlResponse))]
[JsonSerializable(typeof(CloudSongDetailResponse))]
[JsonSerializable(typeof(CloudAlbumResponse))]
[JsonSerializable(typeof(CloudLyricResponse))]
[JsonSerializable(typeof(CloudArtistAlbumResponse))]
[JsonSerializable(typeof(CloudArtistDescriptionResponse))]
[JsonSerializable(typeof(CloudPlaylistDetailResponse))]
internal partial class CloudJsonContext : JsonSerializerContext;
