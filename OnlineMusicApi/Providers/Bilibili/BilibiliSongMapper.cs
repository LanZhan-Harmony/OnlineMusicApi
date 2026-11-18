using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using OnlineMusicApi.Internal.Contracts.Bilibili;

namespace OnlineMusicApi.Internal.Providers.Bilibili;

internal static class BilibiliSongMapper
{
    public static SongId SongIdOf(string key, string? cid = null)
    {
        string value =
            !string.IsNullOrWhiteSpace(cid) && !string.Equals(cid, key, StringComparison.Ordinal)
                ? $"{key}|{cid}"
                : key;

        return new SongId(MusicSource.Bilibili, value);
    }

    public static AlbumId AlbumIdOf(string key) => new(MusicSource.Bilibili, key);

    public static ArtistId ArtistIdOf(string key) => new(MusicSource.Bilibili, key);

    public static Song? FromMedia(BilibiliMediaDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        string key = First(
            dto.Bvid,
            dto.Aid > 0 ? dto.Aid.ToString(CultureInfo.InvariantCulture) : null
        );
        if (key.Length == 0)
        {
            return null;
        }

        string title = CleanTitle(First(dto.Title, dto.Part));
        string author = First(dto.Author, dto.Owner?.Name);

        return new Song
        {
            Id = SongIdOf(key, dto.Cid?.ToString(CultureInfo.InvariantCulture)),
            Title = title,
            Artists = ToArtists(author),
            Album = new AlbumRef { Id = AlbumIdOf(key), Title = title },
            Duration = ParseDuration(dto.DurationText),
            CoverUrl = NormalizeImageUrl(dto.Pic),
            Year = ParseYear(dto.PubDate ?? dto.Created),
        };
    }

    public static Album FromMediaAsAlbum(BilibiliMediaDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        string key = First(
            dto.Bvid,
            dto.Aid > 0 ? dto.Aid.ToString(CultureInfo.InvariantCulture) : null
        );

        return new Album
        {
            Id = AlbumIdOf(key),
            Title = CleanTitle(First(dto.Title, dto.Part)),
            Artists = ToArtists(First(dto.Author, dto.Owner?.Name)),
            Year = ParseYear(dto.PubDate ?? dto.Created),
            CoverUrl = NormalizeImageUrl(dto.Pic),
        };
    }

    public static Artist FromArtist(BilibiliArtistDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Artist
        {
            Id = ArtistIdOf(dto.Mid.ToString(CultureInfo.InvariantCulture)),
            Name = dto.Name ?? string.Empty,
            SongCount = dto.Videos,
            Description = dto.Description ?? string.Empty,
            AvatarUrl = NormalizeImageUrl(dto.Avatar),
        };
    }

    public static Artist FromUserCard(ArtistId id, BilibiliUserCardDto? dto)
    {
        BilibiliUserCardInfoDto? card = dto?.Data?.Card;

        return new Artist
        {
            Id = id,
            Name = card?.Name ?? string.Empty,
            Description = card?.Sign ?? string.Empty,
            AvatarUrl = NormalizeImageUrl(card?.Face),
        };
    }

    public static MediaSource? FromMediaSource(BilibiliMediaSourceDto? dto, AudioQuality requested)
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
            ActualQuality = AudioQuality.Low,
            Codec = "m4a",
            Headers = dto.Headers ?? new Dictionary<string, string>(),
        };
    }

    public static (string Key, string? Cid) ParseSongId(string value)
    {
        int index = value.IndexOf('|', StringComparison.Ordinal);
        return index < 0 ? (value, null) : (value[..index], value[(index + 1)..]);
    }

    public static bool IsAid(string key)
    {
        return long.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
    }

    private static IReadOnlyList<ArtistRef> ToArtists(string? name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? []
            : [new ArtistRef { Id = ArtistIdOf(string.Empty), Name = CleanTitle(name) }];
    }

    private static string CleanTitle(string value)
    {
        return WebUtility.HtmlDecode(Regex.Replace(value, "<.*?>", string.Empty)).Trim();
    }

    private static TimeSpan? ParseDuration(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!value.Contains(':', StringComparison.Ordinal))
        {
            return
                int.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int seconds
                )
                && seconds > 0
                ? TimeSpan.FromSeconds(seconds)
                : null;
        }

        int sum = 0;
        foreach (string part in value.Split(':', StringSplitOptions.RemoveEmptyEntries))
        {
            if (
                !int.TryParse(
                    part,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int number
                )
            )
            {
                continue;
            }

            sum = 60 * sum + number;
        }

        return sum > 0 ? TimeSpan.FromSeconds(sum) : null;
    }

    private static int? ParseYear(long? unixSeconds)
    {
        return unixSeconds is > 0
            ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds.Value).Year
            : null;
    }

    private static string NormalizeImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        return url.StartsWith("//", StringComparison.Ordinal) ? "https:" + url : url;
    }

    private static string First(params string?[] values)
    {
        return values.FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value))
            ?? string.Empty;
    }
}
