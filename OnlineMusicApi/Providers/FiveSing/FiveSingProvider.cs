using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using OnlineMusicApi.Internal.Contracts.FiveSing;
using OnlineMusicApi.Internal.Infrastructure.Caching;
using OnlineMusicApi.Internal.Providers.Abstractions;
using OnlineMusicApi.Internal.Providers.FiveSing.Protocol;
using OnlineMusicApi.Internal.Providers.Shared;

namespace OnlineMusicApi.Internal.Providers.FiveSing;

internal sealed class FiveSingProvider
    : ISongSearchProvider,
        IAlbumSearchProvider,
        IArtistSearchProvider,
        IPlaybackProvider,
        ILyricProvider,
        IAlbumProvider,
        IArtistProvider
{
    private const int PageSize = 10;

    private readonly FiveSingMusicApiService _api;
    private readonly OnlineMusicApiOptions _options;
    private readonly PlaybackCache _playbackCache;
    private readonly ListSnapshotCache _snapshots;

    public FiveSingProvider(
        HttpClient httpClient,
        OnlineMusicApiOptions options,
        PlaybackCache playbackCache,
        ListSnapshotCache snapshots
    )
    {
        _api = new FiveSingMusicApiService(
            httpClient ?? throw new ArgumentNullException(nameof(httpClient))
        );
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _playbackCache = playbackCache ?? throw new ArgumentNullException(nameof(playbackCache));
        _snapshots = snapshots ?? throw new ArgumentNullException(nameof(snapshots));
    }

    public MusicSource Source => MusicSource.FiveSing;

    public string DisplayName => "5sing";

    public ProviderCapability Capabilities =>
        ProviderCapability.SearchSong
        | ProviderCapability.SearchAlbum
        | ProviderCapability.SearchArtist
        | ProviderCapability.Playback
        | ProviderCapability.Lyric
        | ProviderCapability.AlbumDetail
        | ProviderCapability.AlbumSongs
        | ProviderCapability.ArtistDetail;

    public Task<SongPage> SearchSongsAsync(SearchQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        string keyword = ValueNormalizer.NormalizeKeyword(query.Keyword);
        int limit = ValueNormalizer.NormalizeLimit(query.Limit);
        int offset = ValueNormalizer.NormalizeOffset(query.Offset);
        bool probe = query.ProbePlayback ?? _options.EnablePlaybackProbe;

        return PageAssembler.AssembleSongsAsync(
            Source,
            offset,
            limit,
            probe,
            _options.GreedyPageFill,
            _options.MaxGreedyPages,
            (batchOffset, token) => FetchSongSearchBatchAsync(keyword, batchOffset, limit, token),
            ProbeAndFilterAsync,
            cancellationToken
        );
    }

    public async Task<AlbumPage> SearchAlbumsAsync(
        SearchQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        string keyword = ValueNormalizer.NormalizeKeyword(query.Keyword);
        int limit = ValueNormalizer.NormalizeLimit(query.Limit);
        int offset = ValueNormalizer.NormalizeOffset(query.Offset);

        (int page, int skip) = ToPage(offset);
        JsonObject json = await _api.SearchAlbumAsync(keyword, page).ConfigureAwait(false);
        FiveSingAlbumPageDto dto = json.Deserialize(
            FiveSingJsonContext.Default.FiveSingAlbumPageDto
        ) ?? new FiveSingAlbumPageDto();

        List<FiveSingAlbumDto> albums = dto.Data ?? [];
        List<Album> items = [.. albums.Skip(skip).Take(limit).Select(FiveSingSongMapper.FromAlbum)];
        int consumed = Math.Min(limit, Math.Max(0, albums.Count - skip));
        int nextOffset = offset + consumed;
        bool hasMore = !dto.IsEnd || skip + consumed < albums.Count;

        return new AlbumPage
        {
            Source = Source,
            Items = items,
            Total = items.Count,
            NextOffset = hasMore ? nextOffset : null,
            HasMore = hasMore,
        };
    }

    public async Task<ArtistPage> SearchArtistsAsync(
        SearchQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        string keyword = ValueNormalizer.NormalizeKeyword(query.Keyword);
        int limit = ValueNormalizer.NormalizeLimit(query.Limit);
        int offset = ValueNormalizer.NormalizeOffset(query.Offset);

        (int page, int skip) = ToPage(offset);
        JsonObject json = await _api.SearchArtistAsync(keyword, page).ConfigureAwait(false);
        FiveSingArtistPageDto dto = json.Deserialize(
            FiveSingJsonContext.Default.FiveSingArtistPageDto
        ) ?? new FiveSingArtistPageDto();

        List<FiveSingArtistDto> artists = dto.Data ?? [];
        List<Artist> items =
        [
            .. artists.Skip(skip).Take(limit).Select(FiveSingSongMapper.FromArtist),
        ];
        int consumed = Math.Min(limit, Math.Max(0, artists.Count - skip));
        int nextOffset = offset + consumed;
        bool hasMore = !dto.IsEnd || skip + consumed < artists.Count;

        return new ArtistPage
        {
            Source = Source,
            Items = items,
            Total = items.Count,
            NextOffset = hasMore ? nextOffset : null,
            HasMore = hasMore,
        };
    }

    public async Task<MediaSource?> ResolveMediaAsync(
        SongId id,
        AudioQuality quality,
        CancellationToken cancellationToken
    )
    {
        EnsureSource(id.Source);

        MediaSource? media = await _playbackCache
            .GetOrAddAsync(id, token => FetchMediaAsync(id, token), cancellationToken)
            .ConfigureAwait(false);

        return media is null ? null : WithRequestedQuality(media, quality);
    }

    public async Task<Lyric> GetLyricAsync(SongId id, CancellationToken cancellationToken)
    {
        EnsureSource(id.Source);

        (string songId, string? typeEname) = FiveSingSongMapper.ParseSongId(id.Value);
        JsonObject item = new() { ["id"] = songId, ["typeEname"] = typeEname };
        JsonObject json = await _api.GetLyricAsync(item).ConfigureAwait(false);
        FiveSingLyricDto? dto = json.Deserialize(FiveSingJsonContext.Default.FiveSingLyricDto);

        return FiveSingSongMapper.FromLyric(id, dto);
    }

    public async Task<Album> GetAlbumAsync(AlbumId id, CancellationToken cancellationToken)
    {
        EnsureSource(id.Source);

        List<FiveSingSongDto> songs = await FetchAlbumSongsRawAsync(id, cancellationToken)
            .ConfigureAwait(false);
        if (songs.Count == 0)
        {
            throw new OnlineMusicApiResponseException(
                "5sing 歌单没有返回歌曲。",
                nameof(GetAlbumAsync),
                Source
            );
        }

        FiveSingSongDto first = songs[0];
        return new Album
        {
            Id = id,
            Title = first.Album ?? string.Empty,
            SongCount = songs.Count,
            Artists = FiveSingSongMapper.FromSong(first)?.Artists ?? [],
        };
    }

    public Task<SongPage> GetAlbumSongsAsync(
        AlbumId id,
        SongListQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);
        EnsureSource(id.Source);

        int limit = ValueNormalizer.NormalizeLimit(query.Limit);
        int offset = ValueNormalizer.NormalizeOffset(query.Offset);
        bool probe = query.ProbePlayback ?? _options.EnablePlaybackProbe;

        return PageAssembler.AssembleSongsAsync(
            Source,
            offset,
            limit,
            probe,
            _options.GreedyPageFill,
            _options.MaxGreedyPages,
            async (batchOffset, token) =>
            {
                IReadOnlyList<RawSong> snapshot = await GetAlbumSnapshotAsync(id, token)
                    .ConfigureAwait(false);
                return Slice(snapshot, batchOffset, limit);
            },
            ProbeAndFilterAsync,
            cancellationToken
        );
    }

    public async Task<Artist> GetArtistAsync(ArtistId id, CancellationToken cancellationToken)
    {
        EnsureSource(id.Source);

        string html = await _api.GetArtistPageHtmlAsync(id.Value).ConfigureAwait(false);
        return FiveSingPageParser.ParseArtist(html, id);
    }

    public Task<AlbumPage> GetArtistAlbumsAsync(
        ArtistId id,
        PageQuery query,
        CancellationToken cancellationToken
    )
    {
        throw new OnlineMusicApiNotSupportedException(
            nameof(GetArtistAlbumsAsync),
            Source,
            ProviderCapability.ArtistAlbums
        );
    }

    private async Task<RawBatch> FetchSongSearchBatchAsync(
        string keyword,
        int offset,
        int limit,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        (int page, int skip) = ToPage(offset);
        JsonObject json = await _api.SearchMusicAsync(keyword, page).ConfigureAwait(false);
        FiveSingSongPageDto dto = json.Deserialize(FiveSingJsonContext.Default.FiveSingSongPageDto)
            ?? new FiveSingSongPageDto();

        List<FiveSingSongDto> songs = dto.Data ?? [];
        List<RawSong> items = [];
        int consumed = 0;

        for (int index = skip; index < songs.Count && consumed < limit; index++)
        {
            consumed++;
            Song? song = FiveSingSongMapper.FromSong(songs[index]);
            if (song is not null)
            {
                items.Add(new RawSong(song, offset + index - skip));
            }
        }

        int nextOffset = offset + consumed;
        bool hasMore = !dto.IsEnd || skip + consumed < songs.Count;

        return new RawBatch(items, hasMore, nextOffset);
    }

    private async Task<MediaSource?> FetchMediaAsync(SongId id, CancellationToken cancellationToken)
    {
        (string songId, string? typeEname) = FiveSingSongMapper.ParseSongId(id.Value);
        JsonObject item = new() { ["id"] = songId, ["typeEname"] = typeEname };

        JsonObject? json = await _api.GetMediaSourceAsync(item, ToQualityString(AudioQuality.High))
            .ConfigureAwait(false);

        if (json is null)
        {
            return null;
        }

        FiveSingMediaDto? dto = json.Deserialize(FiveSingJsonContext.Default.FiveSingMediaDto);
        return FiveSingSongMapper.FromMedia(dto, AudioQuality.High);
    }

    private async Task<IReadOnlyList<RawSong>> ProbeAndFilterAsync(
        IReadOnlyList<RawSong> items,
        CancellationToken cancellationToken
    )
    {
        using SemaphoreSlim gate = new(
            _options.PlaybackProbeConcurrency,
            _options.PlaybackProbeConcurrency
        );
        ConcurrentBag<RawSong> playable = [];

        Task[] tasks =
        [
            .. items.Select(async raw =>
            {
                await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    MediaSource? media = await _playbackCache
                        .GetOrAddAsync(
                            raw.Song.Id,
                            token => FetchMediaAsync(raw.Song.Id, token),
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    if (media is not null)
                    {
                        playable.Add(
                            new RawSong(
                                WithAvailability(raw.Song, PlaybackAvailability.Playable),
                                raw.RawIndex
                            )
                        );
                    }
                }
                finally
                {
                    gate.Release();
                }
            }),
        ];

        await Task.WhenAll(tasks).ConfigureAwait(false);
        return [.. playable.OrderBy(static item => item.RawIndex)];
    }

    private async Task<IReadOnlyList<RawSong>> GetAlbumSnapshotAsync(
        AlbumId id,
        CancellationToken cancellationToken
    )
    {
        SnapshotKey key = new(Source, SnapshotKind.AlbumSongs, id.Value);
        if (
            _snapshots.TryGet<RawSong>(key, out IReadOnlyList<RawSong>? cached)
            && cached is not null
        )
        {
            return cached;
        }

        List<FiveSingSongDto> songs = await FetchAlbumSongsRawAsync(id, cancellationToken)
            .ConfigureAwait(false);
        RawSong[] snapshot = ToSnapshot(songs);
        _snapshots.Set(key, snapshot);
        return snapshot;
    }

    private async Task<List<FiveSingSongDto>> FetchAlbumSongsRawAsync(
        AlbumId id,
        CancellationToken cancellationToken
    )
    {
        JsonObject albumItem = new() { ["id"] = id.Value };
        JsonObject json = await _api.GetAlbumInfoAsync(albumItem).ConfigureAwait(false);
        FiveSingAlbumSongsDto? dto = json.Deserialize(
            FiveSingJsonContext.Default.FiveSingAlbumSongsDto
        );

        return dto?.MusicList ?? [];
    }

    private static RawSong[] ToSnapshot(List<FiveSingSongDto> songs)
    {
        List<RawSong> snapshot = [with(songs.Count)];
        for (int index = 0; index < songs.Count; index++)
        {
            Song? song = FiveSingSongMapper.FromSong(songs[index]);
            if (song is not null)
            {
                snapshot.Add(new RawSong(song, index));
            }
        }

        return [.. snapshot];
    }

    private static RawBatch Slice(IReadOnlyList<RawSong> snapshot, int offset, int limit)
    {
        int start = Math.Min(offset, snapshot.Count);
        int count = Math.Min(limit, snapshot.Count - start);
        IReadOnlyList<RawSong> items = count > 0 ? [.. snapshot.Skip(start).Take(count)] : [];

        int nextOffset = start + items.Count;
        bool hasMore = nextOffset < snapshot.Count;

        return new RawBatch(items, hasMore, nextOffset);
    }

    private static (int Page, int Skip) ToPage(int offset)
    {
        return (offset / PageSize + 1, offset % PageSize);
    }

    private static string ToQualityString(AudioQuality quality)
    {
        return quality switch
        {
            AudioQuality.Low => "low",
            AudioQuality.Medium => "standard",
            AudioQuality.High => "high",
            AudioQuality.Lossless or AudioQuality.Highest => "super",
            _ => "standard",
        };
    }

    private static MediaSource WithRequestedQuality(MediaSource media, AudioQuality quality)
    {
        return new MediaSource
        {
            Url = media.Url,
            RequestedQuality = quality,
            ActualQuality = media.ActualQuality,
            Codec = media.Codec,
            BitrateKbps = media.BitrateKbps,
            ExpiresAt = media.ExpiresAt,
            Headers = media.Headers,
        };
    }

    private static Song WithAvailability(Song song, PlaybackAvailability availability)
    {
        return new Song
        {
            Id = song.Id,
            Title = song.Title,
            Artists = song.Artists,
            Album = song.Album,
            Duration = song.Duration,
            CoverUrl = song.CoverUrl,
            Year = song.Year,
            Availability = availability,
        };
    }

    private void EnsureSource(MusicSource source)
    {
        if (source != Source)
        {
            throw new ArgumentException(
                $"ID 所属音源 {source} 与 {Source} 不匹配。",
                nameof(source)
            );
        }
    }
}
