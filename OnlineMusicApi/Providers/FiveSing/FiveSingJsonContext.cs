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
internal sealed partial class FiveSingJsonContext : JsonSerializerContext;