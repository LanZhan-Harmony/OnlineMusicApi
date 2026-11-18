using System.Text.Json.Serialization;
using OnlineMusicApi.Internal.Contracts.Bilibili;

namespace OnlineMusicApi.Internal.Providers.Bilibili;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(BilibiliMediaPageDto))]
[JsonSerializable(typeof(BilibiliArtistPageDto))]
[JsonSerializable(typeof(BilibiliAlbumSongsDto))]
[JsonSerializable(typeof(BilibiliMediaSourceDto))]
[JsonSerializable(typeof(BilibiliUserCardDto))]
internal sealed partial class BilibiliJsonContext : JsonSerializerContext;
