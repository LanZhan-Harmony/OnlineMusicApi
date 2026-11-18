using System.Text.Json.Serialization;
using OnlineMusicApi.Internal.Contracts.FiveSing;

namespace OnlineMusicApi.Internal.Providers.FiveSing;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(FiveSingSongPageDto))]
[JsonSerializable(typeof(FiveSingAlbumPageDto))]
[JsonSerializable(typeof(FiveSingArtistPageDto))]
[JsonSerializable(typeof(FiveSingAlbumSongsDto))]
[JsonSerializable(typeof(FiveSingMediaDto))]
[JsonSerializable(typeof(FiveSingLyricDto))]
[JsonSerializable(typeof(FiveSingSearchSongResponseDto))]
[JsonSerializable(typeof(FiveSingSearchAlbumResponseDto))]
[JsonSerializable(typeof(FiveSingSearchArtistResponseDto))]
[JsonSerializable(typeof(FiveSingAlbumSongsResponseDto))]
[JsonSerializable(typeof(FiveSingLyricResponseDto))]
[JsonSerializable(typeof(FiveSingSongUrlResponseDto))]
internal sealed partial class FiveSingJsonContext : JsonSerializerContext;
