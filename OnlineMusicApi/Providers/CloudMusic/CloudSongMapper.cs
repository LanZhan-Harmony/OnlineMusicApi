using System.Globalization;
using OnlineMusicApi.Internal.Contracts.CloudMusic;

namespace OnlineMusicApi.Internal.Providers.CloudMusic;

internal static class CloudSongMapper
{
    public static SongId SongIdOf(long id) =>
        new(MusicSource.CloudMusic, id.ToString(CultureInfo.InvariantCulture));

    public static AlbumId AlbumIdOf(long id) =>
        new(MusicSource.CloudMusic, id.ToString(CultureInfo.InvariantCulture));

    public static ArtistId ArtistIdOf(long id) =>
        new(MusicSource.CloudMusic, id.ToString(CultureInfo.InvariantCulture));

    public static PlaylistId PlaylistIdOf(long id) =>
        new(MusicSource.CloudMusic, id.ToString(CultureInfo.InvariantCulture));

    public static long ParseId(string value)
    {
        return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long id)
            ? id
            : throw new ArgumentException($"无效的网易云 ID：{value}", nameof(value));
    }

    public static Song FromSearchSong(CloudSearchSongDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Song
        {
            Id = SongIdOf(dto.Id),
            Title = dto.Name ?? string.Empty,
            Artists = ToArtistRefs(dto.Artists),
            Album = dto.Album is null
                ? null
                : new AlbumRef
                {
                    Id = AlbumIdOf(dto.Album.Id),
                    Title = dto.Album.Name ?? string.Empty,
                },
            Duration = dto.Duration > 0 ? TimeSpan.FromMilliseconds(dto.Duration) : null,
            CoverUrl = dto.Album?.PicUrl ?? string.Empty,
            Year = ToYear(dto.Album?.PublishTime ?? 0),
        };
    }

    public static Song FromTrack(CloudSongTrackDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Song
        {
            Id = SongIdOf(dto.Id),
            Title = dto.Name ?? string.Empty,
            Artists = ToArtistRefs(dto.Artists),
            Album = dto.Album is null
                ? null
                : new AlbumRef
                {
                    Id = AlbumIdOf(dto.Album.Id),
                    Title = dto.Album.Name ?? string.Empty,
                },
            Duration = dto.Duration > 0 ? TimeSpan.FromMilliseconds(dto.Duration) : null,
            CoverUrl = dto.Album?.PicUrl ?? string.Empty,
            Year = ToYear(dto.Album?.PublishTime ?? 0),
        };
    }

    public static Album FromSearchAlbum(CloudSearchAlbumDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Album
        {
            Id = AlbumIdOf(dto.Id),
            Title = dto.Name ?? string.Empty,
            Artists = ToArtistRefs(dto.Artists),
            Year = ToYear(dto.PublishTime),
            CoverUrl = dto.PicUrl ?? string.Empty,
        };
    }

    public static Album FromAlbumDetail(CloudAlbumDetailDto dto, int songCount)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Album
        {
            Id = AlbumIdOf(dto.Id),
            Title = dto.Name ?? string.Empty,
            Artists = ToArtistRefs(dto.Artists),
            Year = ToYear(dto.PublishTime),
            SongCount = songCount,
            Description = dto.Description ?? string.Empty,
            CoverUrl = dto.PicUrl ?? string.Empty,
        };
    }

    public static Album FromArtistAlbum(CloudArtistAlbumDto dto, CloudArtistDto? artist)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Album
        {
            Id = AlbumIdOf(dto.Id),
            Title = dto.Name ?? string.Empty,
            Artists = artist is null
                ? []
                :
                [
                    new ArtistRef
                    {
                        Id = ArtistIdOf(artist.Id),
                        Name = artist.Name ?? string.Empty,
                    },
                ],
            Year = ToYear(dto.PublishTime),
            CoverUrl = dto.PicUrl ?? string.Empty,
        };
    }

    public static Artist FromSearchArtist(CloudSearchArtistDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Artist
        {
            Id = ArtistIdOf(dto.Id),
            Name = dto.Name ?? string.Empty,
            AvatarUrl = dto.PicUrl ?? string.Empty,
        };
    }

    public static Artist FromArtistSummary(CloudArtistDto dto, string? description)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Artist
        {
            Id = ArtistIdOf(dto.Id),
            Name = dto.Name ?? string.Empty,
            AlbumCount = dto.AlbumSize,
            SongCount = dto.MusicSize,
            Description = description ?? string.Empty,
            AvatarUrl = dto.PicUrl ?? string.Empty,
        };
    }

    public static Playlist FromSearchPlaylist(CloudSearchPlaylistDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new Playlist
        {
            Id = PlaylistIdOf(dto.Id),
            Title = dto.Name ?? string.Empty,
            SongCount = dto.TrackCount,
            CoverUrl = dto.CoverImgUrl ?? string.Empty,
        };
    }

    public static MediaSource? ToMediaSource(CloudSongUrlItem? item)
    {
        if (
            item?.Url is not { Length: > 0 } url
            || !Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
        )
        {
            return null;
        }

        AudioQuality quality = item.BitRate switch
        {
            >= 900_000 => AudioQuality.Lossless,
            >= 300_000 => AudioQuality.High,
            >= 190_000 => AudioQuality.Medium,
            _ => AudioQuality.Low,
        };

        return new MediaSource
        {
            Url = uri,
            RequestedQuality = quality,
            ActualQuality = quality,
            Codec = item.Type,
            BitrateKbps = item.BitRate > 0 ? item.BitRate / 1000 : null,
        };
    }

    public static IReadOnlyList<SearchSuggestion> FromSuggestions(CloudSearchSuggestResult? result)
    {
        if (result is null)
        {
            return [];
        }

        List<SearchSuggestion> suggestions = [];

        AddSuggestions(suggestions, result.Songs, SearchSuggestionKind.Song);
        AddSuggestions(suggestions, result.Albums, SearchSuggestionKind.Album);
        AddSuggestions(suggestions, result.Artists, SearchSuggestionKind.Artist);
        AddSuggestions(suggestions, result.Playlists, SearchSuggestionKind.Playlist);

        return suggestions;
    }

    public static int? ToYear(long unixMilliseconds)
    {
        return unixMilliseconds > 0
            ? DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).Year
            : null;
    }

    private static void AddSuggestions(
        List<SearchSuggestion> target,
        List<CloudSearchSuggestionItemDto>? items,
        SearchSuggestionKind kind
    )
    {
        if (items is null)
        {
            return;
        }

        foreach (CloudSearchSuggestionItemDto item in items)
        {
            if (!string.IsNullOrWhiteSpace(item.Name))
            {
                target.Add(new SearchSuggestion { Kind = kind, Title = item.Name.Trim() });
            }
        }
    }

    private static IReadOnlyList<ArtistRef> ToArtistRefs(
        List<CloudSearchArtistReferenceDto>? artists
    )
    {
        if (artists is null || artists.Count == 0)
        {
            return [];
        }

        List<ArtistRef> result = [with(artists.Count)];
        foreach (CloudSearchArtistReferenceDto artist in artists)
        {
            result.Add(
                new ArtistRef { Id = ArtistIdOf(artist.Id), Name = artist.Name ?? string.Empty }
            );
        }

        return result;
    }
}
