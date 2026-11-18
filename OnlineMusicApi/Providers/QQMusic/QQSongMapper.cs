using OnlineMusicApi.Internal.Contracts.QQMusic;
using OnlineMusicApi.Internal.Providers.Shared;

namespace OnlineMusicApi.Internal.Providers.QQMusic;

internal static class QQSongMapper
{
    public static SongId SongIdOf(string value) => new(MusicSource.QQMusic, value);

    public static AlbumId AlbumIdOf(string value) => new(MusicSource.QQMusic, value);

    public static ArtistId ArtistIdOf(string value) => new(MusicSource.QQMusic, value);

    public static PlaylistId PlaylistIdOf(string value) => new(MusicSource.QQMusic, value);

    public static Song? FromSong(QQSongDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        string id = ValueNormalizer.First(dto.SongMid, dto.Id);
        if (id.Length == 0)
        {
            return null;
        }

        string albumId = ValueNormalizer.First(dto.AlbumMid, dto.AlbumId);
        AlbumRef? album =
            albumId.Length == 0 && string.IsNullOrWhiteSpace(dto.Album)
                ? null
                : new AlbumRef { Id = AlbumIdOf(albumId), Title = dto.Album ?? string.Empty };

        return new Song
        {
            Id = SongIdOf(id),
            Title = dto.Title ?? string.Empty,
            Artists = ToArtists(dto),
            Album = album,
            Duration = dto.Duration > 0 ? TimeSpan.FromMilliseconds(dto.Duration) : null,
            CoverUrl = dto.Artwork ?? string.Empty,
        };
    }

    public static Album FromAlbum(QQAlbumDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        string id = ValueNormalizer.First(dto.AlbumMid, dto.Id);
        string artistId = ValueNormalizer.First(dto.SingerMid, dto.SingerId);

        return new Album
        {
            Id = AlbumIdOf(id),
            Title = dto.Title ?? string.Empty,
            Artists =
                string.IsNullOrWhiteSpace(dto.Artist) && artistId.Length == 0
                    ? []
                    :
                    [
                        new ArtistRef
                        {
                            Id = ArtistIdOf(artistId),
                            Name = dto.Artist ?? string.Empty,
                        },
                    ],
            Year = ParseYear(dto.Date),
            Description = dto.Description ?? string.Empty,
            CoverUrl = dto.Artwork ?? string.Empty,
        };
    }

    public static Artist FromArtist(QQArtistDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Artist
        {
            Id = ArtistIdOf(ValueNormalizer.First(dto.SingerMid, dto.Id)),
            Name = dto.Name ?? string.Empty,
            SongCount = ValueNormalizer.ToInt(dto.WorksNum),
            AvatarUrl = dto.Avatar ?? string.Empty,
        };
    }

    public static Playlist FromPlaylist(QQPlaylistDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Playlist
        {
            Id = PlaylistIdOf(dto.Id ?? string.Empty),
            Title = dto.Title ?? string.Empty,
            SongCount = ValueNormalizer.ToInt(dto.WorksNums),
            Description = dto.Description ?? string.Empty,
            CoverUrl = dto.Artwork ?? string.Empty,
        };
    }

    public static MediaSource? FromMedia(QQMediaDto? dto, AudioQuality quality)
    {
        if (
            dto?.Url is not { Length: > 0 } url
            || !Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
        )
        {
            return null;
        }

        return new MediaSource
        {
            Url = uri,
            RequestedQuality = quality,
            ActualQuality = quality,
            Codec = quality is AudioQuality.Lossless or AudioQuality.Highest ? "flac" : "mp3",
        };
    }

    public static Lyric FromLyric(SongId id, QQLyricDto? dto)
    {
        if (dto?.RawLrc is not { Length: > 0 } text)
        {
            return new Lyric { Id = id, Format = LyricFormat.Unknown };
        }

        return new Lyric
        {
            Id = id,
            Format = LyricFormat.Lrc,
            Text = text,
        };
    }

    public static Artist FromSingerPage(ArtistId id, QQSingerPageDto? dto)
    {
        QQSingerDetailDto? detail = dto?.SingerDetail;

        return new Artist
        {
            Id = id,
            Name = detail?.BasicInfo?.Name ?? detail?.BasicInfo?.SingerMid ?? string.Empty,
            AlbumCount = detail?.AlbumTotalNum ?? detail?.ExInfo?.AlbumTotalNum,
            SongCount = detail?.SongTotalNum ?? detail?.ExInfo?.SongTotalNum,
            Description = ValueNormalizer.First(detail?.ExInfo?.Desc, detail?.ShortDesc),
            AvatarUrl = NormalizeImageUrl(detail?.Pic?.Pic),
        };
    }

    public static Album FromAlbumPage(AlbumId id, QQAlbumDetailPageDto? dto)
    {
        QQAlbumDetailDto? detail = dto?.Detail;
        List<ArtistRef> artists = [];

        if (detail?.Singer is { Count: > 0 })
        {
            foreach (QQAlbumSingerDto singer in detail.Singer)
            {
                artists.Add(
                    new ArtistRef
                    {
                        Id = ArtistIdOf(
                            ValueNormalizer.First(
                                singer.Mid,
                                singer.SingerId.ToString(
                                    System.Globalization.CultureInfo.InvariantCulture
                                )
                            )
                        ),
                        Name = singer.Name ?? string.Empty,
                    }
                );
            }
        }

        return new Album
        {
            Id = id,
            Title = detail?.Title ?? detail?.AlbumName ?? string.Empty,
            Artists = artists,
            Year = ParseYear(detail?.CTime),
            SongCount = dto?.SongList?.Count,
            Description = detail?.Desc ?? string.Empty,
            CoverUrl = NormalizeImageUrl(detail?.PicUrl),
        };
    }

    private static string NormalizeImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        return url.StartsWith("//", StringComparison.Ordinal) ? "https:" + url : url;
    }

    private static IReadOnlyList<ArtistRef> ToArtists(QQSongDto dto)
    {
        if (dto.Singers is { Count: > 0 })
        {
            List<ArtistRef> artists = [with(dto.Singers.Count)];
            foreach (QQSingerDto singer in dto.Singers)
            {
                artists.Add(
                    new ArtistRef
                    {
                        Id = ArtistIdOf(ValueNormalizer.First(singer.Mid, singer.Id)),
                        Name = singer.Name ?? string.Empty,
                    }
                );
            }

            return artists;
        }

        if (string.IsNullOrWhiteSpace(dto.Artist))
        {
            return [];
        }

        return
        [
            .. dto
                .Artist.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                )
                .Select(static name => new ArtistRef
                {
                    Id = ArtistIdOf(string.Empty),
                    Name = name,
                }),
        ];
    }

    private static int? ParseYear(string? value)
    {
        if (
            value is { Length: >= 4 }
            && int.TryParse(
                value.AsSpan(0, 4),
                System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture,
                out int year
            )
        )
        {
            return year;
        }

        return null;
    }
}
