using System.Text.Json.Serialization;
using OnlineMusicApi.Internal.Contracts.KuwoMusic;

namespace OnlineMusicApi.Internal.Providers.KuwoMusic;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(KuwoSongPageDto))]
[JsonSerializable(typeof(KuwoAlbumPageDto))]
[JsonSerializable(typeof(KuwoArtistPageDto))]
[JsonSerializable(typeof(KuwoPlaylistPageDto))]
[JsonSerializable(typeof(KuwoAlbumSongsDto))]
[JsonSerializable(typeof(KuwoMediaDto))]
[JsonSerializable(typeof(KuwoLyricDto))]
internal sealed partial class KuwoJsonContext : JsonSerializerContext;