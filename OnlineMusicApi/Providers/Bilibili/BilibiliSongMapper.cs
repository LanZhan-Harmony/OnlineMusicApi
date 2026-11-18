using OnlineMusicApi.Internal.Contracts.Bilibili;
using OnlineMusicApi.Internal.Providers.Shared;

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

        string key = ValueNormalizer.First(dto.Bvid, dto.Aid);
        if (key.Length == 0)
        {
            return null;
        }

        return new Song
        {
            Id = SongIdOf(key, dto.Cid ?? dto.Id),
            Title = dto.Title ?? string.Empty,
            Artists = ToArtists(dto.Artist),
            Album = new AlbumRef
            {
                Id = AlbumIdOf(key),
                Title = dto.Album ?? dto.Title ?? string.Empty,
            },
            Duration = dto.Duration > 0 ? TimeSpan.FromSeconds(dto.Duration) : null,
            CoverUrl = dto.Artwork ?? string.Empty,
            Year = ParseYear(dto.Date),
        };
    }

    public static Album FromMediaAsAlbum(BilibiliMediaDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        string key = ValueNormalizer.First(dto.Bvid, dto.Aid);

        return new Album
        {
            Id = AlbumIdOf(key),
            Title = dto.Title ?? string.Empty,
            Artists = ToArtists(dto.Artist),
            Year = ParseYear(dto.Date),
            CoverUrl = dto.Artwork ?? string.Empty,
        };
    }

    public static Artist FromArtist(BilibiliArtistDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Artist
        {
            Id = ArtistIdOf(dto.Id ?? string.Empty),
            Name = dto.Name ?? string.Empty,
            SongCount = ValueNormalizer.ToInt(dto.WorksNum),
            Description = dto.Description ?? string.Empty,
            AvatarUrl = dto.Avatar ?? string.Empty,
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
            Headers = dto.Headers ?? [],
        };
    }

    public static (string Key, string? Cid) ParseSongId(string value)
    {
        int index = value.IndexOf('|', StringComparison.Ordinal);
        return index < 0 ? (value, null) : (value[..index], value[(index + 1)..]);
    }

    public static bool IsAid(string key)
    {
        return long.TryParse(
            key,
            System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture,
            out _
        );
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

    private static string NormalizeImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        return url.StartsWith("//", StringComparison.Ordinal) ? "https:" + url : url;
    }

    private static IReadOnlyList<ArtistRef> ToArtists(string? name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? []
            : [new ArtistRef { Id = ArtistIdOf(string.Empty), Name = name.Trim() }];
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
