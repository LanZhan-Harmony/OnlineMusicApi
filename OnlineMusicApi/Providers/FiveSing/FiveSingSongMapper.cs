using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using OnlineMusicApi.Internal.Contracts.FiveSing;

namespace OnlineMusicApi.Internal.Providers.FiveSing;

internal static class FiveSingSongMapper
{
    private const char IdSeparator = '|';

    public static SongId SongIdOf(string id, string? typeEname)
    {
        string value = string.IsNullOrWhiteSpace(typeEname) ? id : $"{id}{IdSeparator}{typeEname}";

        return new SongId(MusicSource.FiveSing, value);
    }

    public static AlbumId AlbumIdOf(string id) => new(MusicSource.FiveSing, id);

    public static ArtistId ArtistIdOf(string id) => new(MusicSource.FiveSing, id);

    public static (string Id, string? TypeEname) ParseSongId(string value)
    {
        int index = value.IndexOf(IdSeparator, StringComparison.Ordinal);
        return index < 0 ? (value, null) : (value[..index], value[(index + 1)..]);
    }

    public static Song? FromSong(FiveSingSongDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        long rawId = dto.SongId > 0 ? dto.SongId : dto.AlbumSongId;
        if (rawId <= 0)
        {
            return null;
        }

        string id = rawId.ToString(CultureInfo.InvariantCulture);
        string typeEname = First(dto.TypeEname, dto.Sk);
        string artist = First(dto.Singer, dto.User?.Name);
        long singerId = dto.SingerId > 0 ? dto.SingerId : dto.User?.Id ?? 0;

        return new Song
        {
            Id = SongIdOf(id, typeEname),
            Title = HtmlToText(First(dto.SongName, dto.SequenceName)),
            Artists =
                artist.Length == 0
                    ? []
                    :
                    [
                        new ArtistRef
                        {
                            Id = ArtistIdOf(singerId.ToString(CultureInfo.InvariantCulture)),
                            Name = HtmlToText(artist),
                        },
                    ],
        };
    }

    public static Album FromAlbum(FiveSingAlbumDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Album
        {
            Id = AlbumIdOf(dto.SongListId ?? string.Empty),
            Title = HtmlToText(dto.Title),
            Artists = string.IsNullOrWhiteSpace(dto.UserName)
                ? []
                :
                [
                    new ArtistRef
                    {
                        Id = ArtistIdOf(string.Empty),
                        Name = HtmlToText(dto.UserName),
                    },
                ],
            Year = ParseYear(dto.CreateTime),
            SongCount = dto.SongCount,
            Description = HtmlToText(dto.Content),
            CoverUrl = dto.PictureUrl ?? string.Empty,
        };
    }

    public static Artist FromArtist(FiveSingArtistDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Artist
        {
            Id = ArtistIdOf(dto.Id.ToString(CultureInfo.InvariantCulture)),
            Name = HtmlToText(dto.NickName),
            SongCount = dto.TotalSong,
            Description = dto.Description ?? string.Empty,
            AvatarUrl = dto.PictureUrl ?? string.Empty,
        };
    }

    public static MediaSource? FromMedia(FiveSingMediaDto? dto, AudioQuality requested)
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
            RequestedQuality = requested,
            ActualQuality = requested,
            Codec = "mp3",
        };
    }

    public static Lyric FromLyric(SongId id, FiveSingLyricDto? dto)
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

    private static string HtmlToText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        return WebUtility.HtmlDecode(Regex.Replace(html, "<.*?>", string.Empty)).Trim();
    }

    private static string First(params string?[] values)
    {
        return values.FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value))
            ?? string.Empty;
    }

    private static int? ParseYear(string? value)
    {
        if (
            value is { Length: >= 4 }
            && int.TryParse(
                value.AsSpan(0, 4),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int year
            )
        )
        {
            return year;
        }

        return null;
    }
}
