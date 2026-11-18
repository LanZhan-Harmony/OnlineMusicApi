using OnlineMusicApi.Internal.Contracts.FiveSing;
using OnlineMusicApi.Internal.Providers.Shared;

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

        if (string.IsNullOrWhiteSpace(dto.Id))
        {
            return null;
        }

        return new Song
        {
            Id = SongIdOf(dto.Id, dto.TypeEname ?? dto.Type),
            Title = dto.Title ?? string.Empty,
            Artists = string.IsNullOrWhiteSpace(dto.Artist)
                ? []
                :
                [
                    new ArtistRef
                    {
                        Id = ArtistIdOf(dto.SingerId ?? string.Empty),
                        Name = dto.Artist,
                    },
                ],
        };
    }

    public static Album FromAlbum(FiveSingAlbumDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Album
        {
            Id = AlbumIdOf(dto.Id ?? string.Empty),
            Title = dto.Title ?? string.Empty,
            Artists = string.IsNullOrWhiteSpace(dto.Artist)
                ? []
                : [new ArtistRef { Id = ArtistIdOf(string.Empty), Name = dto.Artist }],
            Year = ParseYear(dto.Date),
            Description = dto.Description ?? string.Empty,
            CoverUrl = dto.Artwork ?? string.Empty,
        };
    }

    public static Artist FromArtist(FiveSingArtistDto dto)
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
