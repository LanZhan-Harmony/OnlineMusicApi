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

        if (string.IsNullOrWhiteSpace(dto.Id))
        {
            return null;
        }

        return new Song
        {
            Id = SongIdOf(dto.Id),
            Title = dto.Title ?? string.Empty,
            Artists = ToArtists(dto.Artist, dto.ArtistId),
            Album =
                string.IsNullOrWhiteSpace(dto.AlbumId) && string.IsNullOrWhiteSpace(dto.Album)
                    ? null
                    : new AlbumRef
                    {
                        Id = AlbumIdOf(dto.AlbumId ?? string.Empty),
                        Title = dto.Album ?? string.Empty,
                    },
            Duration = dto.Duration > 0 ? TimeSpan.FromMilliseconds(dto.Duration) : null,
            CoverUrl = dto.Artwork ?? string.Empty,
        };
    }

    public static Album FromAlbum(KuwoAlbumDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Album
        {
            Id = AlbumIdOf(dto.Id ?? string.Empty),
            Title = dto.Title ?? string.Empty,
            Artists = string.IsNullOrWhiteSpace(dto.Artist)
                ? []
                :
                [
                    new ArtistRef
                    {
                        Id = ArtistIdOf(dto.ArtistId ?? string.Empty),
                        Name = dto.Artist,
                    },
                ],
            Year = ParseYear(dto.Date),
            Description = dto.Description ?? string.Empty,
            CoverUrl = dto.Artwork ?? string.Empty,
        };
    }

    public static Artist FromArtist(KuwoArtistDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Artist
        {
            Id = ArtistIdOf(dto.ArtistId ?? dto.Id ?? string.Empty),
            Name = dto.Name ?? string.Empty,
            SongCount = ParseNumber(dto.WorksNum),
            Description = dto.Description ?? string.Empty,
            AvatarUrl = dto.Avatar ?? string.Empty,
        };
    }

    public static Playlist FromPlaylist(KuwoPlaylistDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Playlist
        {
            Id = PlaylistIdOf(dto.Id ?? string.Empty),
            Title = dto.Title ?? string.Empty,
            SongCount = ParseNumber(dto.WorksNum),
            Description = dto.Description ?? string.Empty,
            CoverUrl = dto.Artwork ?? string.Empty,
        };
    }

    public static MediaSource? FromMedia(KuwoMediaDto? dto)
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
            ActualQuality = AudioQuality.Low,
            Codec = "mp3",
        };
    }

    public static Lyric FromLyric(SongId id, KuwoLyricDto? dto)
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

    private static IReadOnlyList<ArtistRef> ToArtists(string? names, string? ids)
    {
        string[] nameArray = (names ?? string.Empty).Split(
            '&',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        if (nameArray.Length == 0)
        {
            return [];
        }

        string[] idArray = (ids ?? string.Empty).Split(
            '&',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        List<ArtistRef> artists = [with(nameArray.Length)];
        for (int index = 0; index < nameArray.Length; index++)
        {
            artists.Add(
                new ArtistRef
                {
                    Id = ArtistIdOf(index < idArray.Length ? idArray[index] : string.Empty),
                    Name = nameArray[index],
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

    private static int? ParseNumber(string? value)
    {
        return int.TryParse(
            value,
            System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture,
            out int number
        )
            ? number
            : null;
    }
}
