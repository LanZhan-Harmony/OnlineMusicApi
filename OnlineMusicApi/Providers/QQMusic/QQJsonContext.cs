using System.Text.Json.Serialization;
using OnlineMusicApi.Internal.Contracts.QQMusic;

namespace OnlineMusicApi.Internal.Providers.QQMusic;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(QQSongPageDto))]
[JsonSerializable(typeof(QQAlbumPageDto))]
[JsonSerializable(typeof(QQArtistPageDto))]
[JsonSerializable(typeof(QQPlaylistPageDto))]
[JsonSerializable(typeof(QQAlbumSongsDto))]
[JsonSerializable(typeof(QQMediaDto))]
[JsonSerializable(typeof(QQLyricDto))]
[JsonSerializable(typeof(QQSingerPageDto))]
[JsonSerializable(typeof(QQAlbumDetailPageDto))]
[JsonSerializable(typeof(QQSearchSongResponseDto))]
[JsonSerializable(typeof(QQMusicuSearchResponseDto))]
[JsonSerializable(typeof(QQAlbumSongListResponseDto))]
[JsonSerializable(typeof(QQSingerAlbumResponseDto))]
[JsonSerializable(typeof(QQPlaylistResponseDto))]
[JsonSerializable(typeof(QQLyricResponseDto))]
[JsonSerializable(typeof(QQVKeyResponseDto))]
[JsonSerializable(typeof(QQCommRequestDto))]
[JsonSerializable(typeof(QQMusicuSearchRequestDto))]
[JsonSerializable(typeof(QQAlbumSongListRequestDto))]
[JsonSerializable(typeof(QQSingerAlbumRequestDto))]
[JsonSerializable(typeof(QQVKeyRequestDto))]
internal sealed partial class QQJsonContext : JsonSerializerContext;
