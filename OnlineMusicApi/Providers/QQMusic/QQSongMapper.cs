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

        string id = ValueNormalizer.First(dto.Mid, dto.SongMid);
        if (id.Length == 0)
        {
            return null;
        }

        string albumMid = ValueNormalizer.First(dto.Album?.Mid, dto.AlbumMid);
        string albumTitle = ValueNormalizer.First(dto.Album?.Name, dto.Album?.Title, dto.AlbumName);

        return new Song
        {
            Id = SongIdOf(id),
            Title = ValueNormalizer.First(dto.Name, dto.SongName, dto.Title),
            Artists = ToArtists(dto.Singers),
            Album =
                albumMid.Length == 0 && albumTitle.Length == 0
                    ? null
                    : new AlbumRef { Id = AlbumIdOf(albumMid), Title = albumTitle },
            Duration = dto.Interval > 0 ? TimeSpan.FromSeconds(dto.Interval) : null,
            CoverUrl = BuildCoverUrl(albumMid),
            Year = ParseYear(ValueNormalizer.FirstOrNull(dto.TimePublic, dto.Album?.TimePublic)),
        };
    }

    public static Album FromAlbum(QQAlbumDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        string albumMid = ValueNormalizer.First(dto.AlbumMid, dto.AlbumMidUpper, dto.AlbumMidLower);
        string albumId =
            albumMid.Length > 0
                ? albumMid
                : ValueNormalizer.First(
                    dto.AlbumId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    dto.AlbumIdUpper.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    dto.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)
                );

        string singerName = ValueNormalizer.First(dto.SingerName, dto.SingerNameUpper);
        string singerMid = ValueNormalizer.First(dto.SingerMid, dto.SingerMidUpper);
        string cover = ValueNormalizer.First(dto.AlbumPic, dto.AlbumPicLower, dto.Pic);
        if (cover.Length == 0)
        {
            cover = BuildCoverUrl(albumMid);
        }

        return new Album
        {
            Id = AlbumIdOf(albumId),
            Title = ValueNormalizer.First(dto.AlbumName, dto.AlbumNameUpper, dto.Name, dto.Title),
            Artists =
                singerName.Length == 0
                    ? []
                    : [new ArtistRef { Id = ArtistIdOf(singerMid), Name = singerName }],
            Year = ParseYear(ValueNormalizer.First(dto.PubTime, dto.PublicTime, dto.TimePublic)),
            SongCount = dto.SongCount,
            Description = ValueNormalizer.First(dto.Desc, dto.Description),
            CoverUrl = cover,
        };
    }

    public static Artist FromArtist(QQArtistDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Artist
        {
            Id = ArtistIdOf(
                ValueNormalizer.First(
                    dto.SingerMid,
                    dto.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)
                )
            ),
            Name = ValueNormalizer.First(dto.Name, dto.SingerName),
            AlbumCount = dto.AlbumSize,
            SongCount = dto.MusicSize ?? dto.WorksNum,
            AvatarUrl = NormalizeImageUrl(ValueNormalizer.First(dto.Avatar, dto.SingerPic)),
        };
    }

    public static Playlist FromPlaylist(QQPlaylistDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        long playlistId = dto.DissId > 0 ? dto.DissId : dto.Id;

        return new Playlist
        {
            Id = PlaylistIdOf(
                playlistId.ToString(System.Globalization.CultureInfo.InvariantCulture)
            ),
            Title = ValueNormalizer.First(dto.DissName, dto.Title),
            SongCount = dto.SongCount ?? dto.WorksNums,
            Description = ValueNormalizer.First(dto.ProgramDesc, dto.Description),
            CoverUrl = ValueNormalizer.First(dto.ImgUrl, dto.Artwork),
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
            Title = ValueNormalizer.First(detail?.Title, detail?.AlbumName),
            Artists = artists,
            Year = ParseYear(detail?.CTime),
            SongCount = dto?.SongList?.Count,
            Description = detail?.Desc ?? string.Empty,
            CoverUrl = NormalizeImageUrl(detail?.PicUrl),
        };
    }

    private static string BuildCoverUrl(string albumMid)
    {
        return albumMid.Length == 0
            ? string.Empty
            : $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{albumMid}.jpg";
    }

    private static string NormalizeImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        return url.StartsWith("//", StringComparison.Ordinal) ? "https:" + url : url;
    }

    private static IReadOnlyList<ArtistRef> ToArtists(List<QQSingerDto>? singers)
    {
        if (singers is not { Count: > 0 })
        {
            return [];
        }

        List<ArtistRef> artists = new(singers.Count);
        foreach (QQSingerDto singer in singers)
        {
            artists.Add(
                new ArtistRef
                {
                    Id = ArtistIdOf(
                        ValueNormalizer.First(
                            singer.Mid,
                            singer.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        )
                    ),
                    Name = singer.Name ?? string.Empty,
                }
            );
        }

        return artists;
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
