using System.Text.Json.Serialization;
using OnlineMusicApi.Internal.Contracts.KuwoMusic;

namespace OnlineMusicApi.Internal.Providers.KuwoMusic;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = false)]
[JsonSerializable(typeof(KuwoSongPageDto))]
[JsonSerializable(typeof(KuwoAlbumPageDto))]
[JsonSerializable(typeof(KuwoArtistPageDto))]
[JsonSerializable(typeof(KuwoPlaylistPageDto))]
[JsonSerializable(typeof(KuwoAlbumSongsDto))]
[JsonSerializable(typeof(KuwoMediaDto))]
[JsonSerializable(typeof(KuwoLyricDto))]
[JsonSerializable(typeof(KuwoMusicSearchResponseDto))]
[JsonSerializable(typeof(KuwoAlbumSearchResponseDto))]
[JsonSerializable(typeof(KuwoArtistSearchResponseDto))]
[JsonSerializable(typeof(KuwoPlaylistSearchResponseDto))]
[JsonSerializable(typeof(KuwoAlbumInfoResponseDto))]
[JsonSerializable(typeof(KuwoArtistAlbumResponseDto))]
[JsonSerializable(typeof(KuwoPlaylistResponseDto))]
[JsonSerializable(typeof(KuwoLyricResponseDto))]
[JsonSerializable(typeof(KuwoMediaResponseDto))]
internal sealed partial class KuwoJsonContext : JsonSerializerContext;
