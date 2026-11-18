using System.Text.Json.Serialization;
using OnlineMusicApi.Internal.Contracts.Bilibili;

namespace OnlineMusicApi.Internal.Providers.Bilibili;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(BilibiliMediaPageDto))]
[JsonSerializable(typeof(BilibiliArtistPageDto))]
[JsonSerializable(typeof(BilibiliAlbumSongsDto))]
[JsonSerializable(typeof(BilibiliMediaSourceDto))]
[JsonSerializable(typeof(BilibiliUserCardDto))]
[JsonSerializable(typeof(BilibiliSearchVideoResponseDto))]
[JsonSerializable(typeof(BilibiliUserSearchResponseDto))]
[JsonSerializable(typeof(BilibiliViewResponseDto))]
[JsonSerializable(typeof(BilibiliSpaceResponseDto))]
[JsonSerializable(typeof(BilibiliPlayUrlResponseDto))]
[JsonSerializable(typeof(BilibiliFingerResponseDto))]
[JsonSerializable(typeof(BilibiliTicketResponseDto))]
internal sealed partial class BilibiliJsonContext : JsonSerializerContext;
