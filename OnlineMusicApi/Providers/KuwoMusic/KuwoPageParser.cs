using System.Globalization;
using System.Net;

namespace OnlineMusicApi.Internal.Providers.KuwoMusic;

internal static class KuwoPageParser
{
    public static Artist ParseArtist(string html, ArtistId id)
    {
        IReadOnlyDictionary<string, object?> root = NuxtExpressionParser.Parse(html);
        Dictionary<string, object?>? singer = GetObject(GetPage(root), "singerInfo");

        return new Artist
        {
            Id = id,
            Name = Decode(GetString(singer, "name")),
            AlbumCount = GetInt(singer, "albumNum"),
            SongCount = GetInt(singer, "musicNum"),
            Description = Decode(GetString(singer, "info")),
            AvatarUrl = Decode(
                First(
                    GetString(singer, "pic300"),
                    GetString(singer, "pic120"),
                    GetString(singer, "pic")
                )
            ),
        };
    }

    public static Album ParseAlbum(string html, AlbumId id)
    {
        IReadOnlyDictionary<string, object?> root = NuxtExpressionParser.Parse(html);
        Dictionary<string, object?>? album = GetObject(GetPage(root), "albumInfo");
        List<object?>? songs = GetList(album, "musiclist");
        Dictionary<string, object?>? first = songs?.FirstOrDefault() as Dictionary<string, object?>;

        string artist = First(
            GetString(album, "artist"),
            GetString(first, "artist"),
            GetString(album, "fartist")
        );
        string artistId = First(
            GetString(album, "artistid"),
            GetString(first, "artistid"),
            GetString(first, "allartistid")
        );

        return new Album
        {
            Id = id,
            Title = Decode(First(GetString(album, "name"), GetString(first, "album"))),
            Artists = string.IsNullOrWhiteSpace(artist)
                ? []
                :
                [
                    new ArtistRef
                    {
                        Id = KuwoSongMapper.ArtistIdOf(artistId),
                        Name = Decode(artist),
                    },
                ],
            Year = ParseYear(
                First(
                    GetString(album, "releaseDate"),
                    GetString(album, "pub"),
                    GetString(first, "releasedate")
                )
            ),
            SongCount = GetInt(album, "songnum") ?? songs?.Count,
            Description = Decode(GetString(album, "info")),
            CoverUrl = Decode(
                First(
                    GetString(album, "hts_img"),
                    GetString(album, "img"),
                    GetString(first, "pic120")
                )
            ),
        };
    }

    private static Dictionary<string, object?>? GetPage(IReadOnlyDictionary<string, object?> root)
    {
        return root.GetValueOrDefault("data") is List<object?> data && data.Count > 0
            ? data[0] as Dictionary<string, object?>
            : null;
    }

    private static Dictionary<string, object?>? GetObject(
        Dictionary<string, object?>? source,
        string key
    )
    {
        return source?.GetValueOrDefault(key) as Dictionary<string, object?>;
    }

    private static List<object?>? GetList(Dictionary<string, object?>? source, string key)
    {
        return source?.GetValueOrDefault(key) as List<object?>;
    }

    private static string? GetString(Dictionary<string, object?>? source, string key)
    {
        object? value = source?.GetValueOrDefault(key);

        return value switch
        {
            string text => text,
            long number => number.ToString(CultureInfo.InvariantCulture),
            double number => number.ToString(CultureInfo.InvariantCulture),
            bool flag => flag ? "1" : "0",
            _ => null,
        };
    }

    private static int? GetInt(Dictionary<string, object?>? source, string key)
    {
        return int.TryParse(
            GetString(source, key),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out int value
        )
            ? value
            : null;
    }

    private static string First(params string?[] values)
    {
        return values.FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value))
            ?? string.Empty;
    }

    private static string Decode(string? value)
    {
        return WebUtility.HtmlDecode(value ?? string.Empty);
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
