using System.Globalization;
using System.Net;
using OnlineMusicApi.Internal.Contracts.KuwoMusic;

namespace OnlineMusicApi.Internal.Providers.KuwoMusic;

internal static class KuwoSongMapper
{
    public static SongId SongIdOf(string value) => new(MusicSource.KuwoMusic, value);

    public static AlbumId AlbumIdOf(string value) => new(MusicSource.KuwoMusic, value);

    public static ArtistId ArtistIdOf(string value) => new(MusicSource.KuwoMusic, value);

    public static PlaylistId PlaylistIdOf(string value) => new(MusicSource.KuwoMusic, value);

    public static Song? FromSong(KuwoSongDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        string id = NormalizeSongId(
            First(dto.MusicRid, dto.MusicRidLower, dto.Id));
        if (id.Length == 0)
        {
            return null;
        }

        string albumId = First(
            dto.AlbumId,
            dto.AlbumIdLower,
            dto.AlbumIdNumber?.ToString(CultureInfo.InvariantCulture));
        string albumTitle = First(dto.Album, dto.AlbumLower);

        return new Song
        {
            Id = SongIdOf(id),
            Title = HtmlDecode(First(dto.Name, dto.SongName, dto.NameLower)),
            Artists = ToArtists(dto),
            Album = albumId.Length == 0 && albumTitle.Length == 0
                ? null
                : new AlbumRef
                {
                    Id = AlbumIdOf(albumId),
                    Title = HtmlDecode(albumTitle)
                },
            Duration = ToDuration(First(dto.Duration, dto.DurationLower)),
            CoverUrl = First(
                ArtworkShortToLong(dto.ArtworkShort),
                dto.Pic120,
                dto.Img),
            Year = ParseYear(dto.ReleaseDate),
        };
    }

    public static Album FromAlbum(KuwoAlbumDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        string artist = First(dto.Artist, dto.FullArtist);
        string artistId = dto.ArtistId ?? string.Empty;

        return new Album
        {
            Id = AlbumIdOf(First(dto.AlbumId, dto.Id)),
            Title = HtmlDecode(First(dto.Name, dto.Album, dto.Title)),
            Artists = artist.Length == 0
                ? []
                :
                [
                    new ArtistRef
                    {
                        Id = ArtistIdOf(artistId),
                        Name = HtmlDecode(artist)
                    }
                ],
            Year = ParseYear(dto.Pub),
            SongCount = ParseNullableInt(First(dto.SongNum, dto.MusicCount)),
            Description = HtmlDecode(dto.Info),
            CoverUrl = First(
                dto.HtsImg,
                dto.Img,
                ArtworkShortToLong(dto.Pic)),
        };
    }

    public static Artist FromArtist(KuwoArtistDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Artist
        {
            Id = ArtistIdOf(dto.ArtistId ?? string.Empty),
            Name = HtmlDecode(dto.Artist),
            AlbumCount = ParseNullableInt(dto.AlbumNum),
            SongCount = ParseNullableInt(dto.SongNum),
            Description = HtmlDecode(dto.Description),
            AvatarUrl = First(dto.HtsPicPath, BuildArtistPic(dto.PicPath)),
        };
    }

    public static Playlist FromPlaylist(KuwoPlaylistDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Playlist
        {
            Id = PlaylistIdOf(dto.PlaylistId ?? string.Empty),
            Title = HtmlDecode(First(dto.Name, dto.Title)),
            SongCount = ParseNullableInt(dto.SongNum),
            Description = HtmlDecode(dto.Intro),
            CoverUrl = First(dto.HtsPic, dto.Pic),
        };
    }

    public static MediaSource? FromMedia(KuwoMediaDto? dto)
    {
        if (dto?.Url is not { Length: > 0 } url
            || !Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            return null;
        }

        return new MediaSource
        {
            Url = uri,
            ActualQuality = AudioQuality.Low,
            Codec = "mp3",
        };
    }

    public static Lyric FromLyric(SongId id, KuwoLyricDto? dto)
    {
        if (dto?.RawLrc is not { Length: > 0 } text)
        {
            return new Lyric
            {
                Id = id,
                Format = LyricFormat.Unknown
            };
        }

        return new Lyric
        {
            Id = id,
            Format = LyricFormat.Lrc,
            Text = text
        };
    }

    private static IReadOnlyList<ArtistRef> ToArtists(KuwoSongDto dto)
    {
        string[] names = First(dto.Artist, dto.ArtistLower).Split(
            '&',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (names.Length == 0)
        {
            return [];
        }

        string[] ids = First(dto.ArtistId, dto.ArtistIdLower).Split(
            '&',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<ArtistRef> artists = new(names.Length);
        for (int index = 0; index < names.Length; index++)
        {
            artists.Add(new ArtistRef
            {
                Id = ArtistIdOf(index < ids.Length ? ids[index] : string.Empty),
                Name = HtmlDecode(names[index]),
            });
        }

        return artists;
    }

    private static string NormalizeSongId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.StartsWith("MUSIC_", StringComparison.Ordinal)
            ? value["MUSIC_".Length..]
            : value;
    }

    private static string BuildArtistPic(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? path
            : "https://img4.kuwo.cn/star/starheads/" + path;
    }

    private static string? ArtworkShortToLong(string? albumPicShort)
    {
        if (string.IsNullOrWhiteSpace(albumPicShort))
        {
            return null;
        }

        int firstSlash = albumPicShort.IndexOf('/', StringComparison.Ordinal);
        return firstSlash == -1
            ? null
            : $"https://img4.kuwo.cn/star/albumcover/256{albumPicShort[firstSlash..]}";
    }

    private static string HtmlDecode(string? text)
    {
        return WebUtility.HtmlDecode(text ?? string.Empty);
    }

    private static string First(params string?[] values)
    {
        return values.FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value))
            ?? string.Empty;
    }

    private static TimeSpan? ToDuration(string? value)
    {
        return ParseInt(value) is > 0 and int seconds
            ? TimeSpan.FromSeconds(seconds)
            : null;
    }

    private static int? ParseNullableInt(string? value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number)
            ? number
            : null;
    }

    private static int ParseInt(string? value)
    {
        return ParseNullableInt(value) ?? 0;
    }

    private static int? ParseYear(string? value)
    {
        if (value is { Length: >= 4 }
            && int.TryParse(
                value.AsSpan(0, 4),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int year))
        {
            return year;
        }

        return null;
    }
}
