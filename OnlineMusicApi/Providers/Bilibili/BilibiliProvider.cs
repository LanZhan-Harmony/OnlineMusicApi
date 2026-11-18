using System.Collections.Concurrent;
using OnlineMusicApi.Internal.Contracts.Bilibili;
using OnlineMusicApi.Internal.Infrastructure.Caching;
using OnlineMusicApi.Internal.Providers.Abstractions;
using OnlineMusicApi.Internal.Providers.Bilibili.Protocol;
using OnlineMusicApi.Internal.Providers.Shared;

namespace OnlineMusicApi.Internal.Providers.Bilibili;

internal sealed class BilibiliProvider
    : ISongSearchProvider,
        IAlbumSearchProvider,
        IArtistSearchProvider,
        IPlaybackProvider,
        IAlbumProvider,
        IArtistProvider
{
    private const int PageSize = 20;

    private readonly BilibiliMusicApiService _api;
    private readonly OnlineMusicApiOptions _options;
    private readonly PlaybackCache _playbackCache;
    private readonly ListSnapshotCache _snapshots;

    public BilibiliProvider(
        HttpClient httpClient,
        OnlineMusicApiOptions options,
        PlaybackCache playbackCache,
        ListSnapshotCache snapshots
    )
    {
        _api = new BilibiliMusicApiService(
            httpClient ?? throw new ArgumentNullException(nameof(httpClient))
        );
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _playbackCache = playbackCache ?? throw new ArgumentNullException(nameof(playbackCache));
        _snapshots = snapshots ?? throw new ArgumentNullException(nameof(snapshots));
    }

    public MusicSource Source => MusicSource.Bilibili;

    public string DisplayName => "哔哩哔哩";

    public ProviderCapability Capabilities =>
        ProviderCapability.SearchSong
        | ProviderCapability.SearchAlbum
        | ProviderCapability.SearchArtist
        | ProviderCapability.Playback
        | ProviderCapability.AlbumDetail
        | ProviderCapability.AlbumSongs
        | ProviderCapability.ArtistDetail
        | ProviderCapability.ArtistAlbums;

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
        BilibiliMediaPageDto dto = await _api.SearchAlbumAsync(keyword, page).ConfigureAwait(false);

        List<BilibiliMediaDto> medias = dto.Data ?? [];
        List<Album> items =
        [
            .. medias.Skip(skip).Take(limit).Select(BilibiliSongMapper.FromMediaAsAlbum),
        ];
        int consumed = Math.Min(limit, Math.Max(0, medias.Count - skip));
        int nextOffset = offset + consumed;
        bool hasMore = !dto.IsEnd || skip + consumed < medias.Count;

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
        BilibiliArtistPageDto dto = await _api.SearchArtistAsync(keyword, page)
            .ConfigureAwait(false);

        List<BilibiliArtistDto> artists = dto.Data ?? [];
        List<Artist> items =
        [
            .. artists.Skip(skip).Take(limit).Select(BilibiliSongMapper.FromArtist),
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

    public async Task<Album> GetAlbumAsync(AlbumId id, CancellationToken cancellationToken)
    {
        EnsureSource(id.Source);

        BilibiliAlbumSongsDto songsDto = await _api.GetAlbumSongsAsync(id.Value)
            .ConfigureAwait(false);
        List<BilibiliMediaDto> medias = songsDto.MusicList ?? [];

        if (medias.Count == 0)
        {
            throw new OnlineMusicApiResponseException(
                "Bilibili 视频没有返回分 P 信息。",
                nameof(GetAlbumAsync),
                Source
            );
        }

        Song? firstSong = BilibiliSongMapper.FromMedia(medias[0]);

        return new Album
        {
            Id = id,
            Title = firstSong?.Title ?? string.Empty,
            Artists = firstSong?.Artists ?? [],
            Year = firstSong?.Year,
            SongCount = medias.Count,
            CoverUrl = firstSong?.CoverUrl ?? string.Empty,
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

        BilibiliUserCardDto dto = await _api.GetUserCardAsync(id.Value).ConfigureAwait(false);

        if (dto?.Data?.Card is null)
        {
            throw new OnlineMusicApiResponseException(
                "Bilibili UP 主接口没有返回资料。",
                nameof(GetArtistAsync),
                Source
            );
        }

        return BilibiliSongMapper.FromUserCard(id, dto);
    }

    public async Task<AlbumPage> GetArtistAlbumsAsync(
        ArtistId id,
        PageQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);
        EnsureSource(id.Source);

        int limit = ValueNormalizer.NormalizeLimit(query.Limit);
        int offset = ValueNormalizer.NormalizeOffset(query.Offset);
        (int page, int skip) = ToPage(offset);

        BilibiliMediaPageDto dto = await _api.GetArtistWorksAsync(id.Value, page)
            .ConfigureAwait(false);

        List<BilibiliMediaDto> medias = dto.Data ?? [];
        List<Album> items =
        [
            .. medias.Skip(skip).Take(limit).Select(BilibiliSongMapper.FromMediaAsAlbum),
        ];
        int consumed = Math.Min(limit, Math.Max(0, medias.Count - skip));
        int nextOffset = offset + consumed;
        bool hasMore = !dto.IsEnd || skip + consumed < medias.Count;

        return new AlbumPage
        {
            Source = Source,
            Items = items,
            Total = items.Count,
            NextOffset = hasMore ? nextOffset : null,
            HasMore = hasMore,
        };
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
        BilibiliMediaPageDto dto = await _api.SearchMusicAsync(keyword, page).ConfigureAwait(false);

        List<BilibiliMediaDto> medias = dto.Data ?? [];
        List<RawSong> items = [];
        int consumed = 0;

        for (int index = skip; index < medias.Count && consumed < limit; index++)
        {
            consumed++;
            Song? song = BilibiliSongMapper.FromMedia(medias[index]);
            if (song is not null)
            {
                items.Add(new RawSong(song, offset + index - skip));
            }
        }

        int nextOffset = offset + consumed;
        bool hasMore = !dto.IsEnd || skip + consumed < medias.Count;

        return new RawBatch(items, hasMore, nextOffset);
    }

    private async Task<MediaSource?> FetchMediaAsync(SongId id, CancellationToken cancellationToken)
    {
        (string key, string? cid) = BilibiliSongMapper.ParseSongId(id.Value);

        BilibiliMediaSourceDto? dto = await _api.GetMediaSourceAsync(
                key,
                cid,
                ToQualityString(AudioQuality.Highest)
            )
            .ConfigureAwait(false);

        return BilibiliSongMapper.FromMediaSource(dto, AudioQuality.Highest);
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

        BilibiliAlbumSongsDto dto = await _api.GetAlbumSongsAsync(id.Value).ConfigureAwait(false);
        RawSong[] snapshot = ToSnapshot(dto.MusicList ?? []);
        _snapshots.Set(key, snapshot);
        return snapshot;
    }

    private static RawSong[] ToSnapshot(List<BilibiliMediaDto> medias)
    {
        List<RawSong> snapshot = [with(medias.Count)];
        for (int index = 0; index < medias.Count; index++)
        {
            Song? song = BilibiliSongMapper.FromMedia(medias[index]);
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
