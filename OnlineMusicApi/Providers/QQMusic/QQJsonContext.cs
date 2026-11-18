using System.Text.Json.Serialization;
using OnlineMusicApi.Internal.Contracts.QQMusic;

namespace OnlineMusicApi.Internal.Providers.QQMusic;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(QQSongPageDto))]
[JsonSerializable(typeof(QQAlbumPageDto))]
[JsonSerializable(typeof(QQSingerPageDto))]
[JsonSerializable(typeof(QQAlbumDetailPageDto))]
[JsonSerializable(typeof(QQArtistPageDto))]
[JsonSerializable(typeof(QQPlaylistPageDto))]
[JsonSerializable(typeof(QQAlbumSongsDto))]
[JsonSerializable(typeof(QQMediaDto))]
[JsonSerializable(typeof(QQLyricDto))]
internal sealed partial class QQJsonContext : JsonSerializerContext;