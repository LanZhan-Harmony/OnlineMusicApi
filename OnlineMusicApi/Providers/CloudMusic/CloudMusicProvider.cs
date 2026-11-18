using OnlineMusicApi.Internal.Contracts.CloudMusic;
using OnlineMusicApi.Internal.Infrastructure.Caching;
using OnlineMusicApi.Internal.Providers.Abstractions;
using OnlineMusicApi.Internal.Providers.CloudMusic.Protocol;
using OnlineMusicApi.Internal.Providers.Shared;

namespace OnlineMusicApi.Internal.Providers.CloudMusic;

internal sealed class CloudMusicProvider
    : ISongSearchProvider,
        IAlbumSearchProvider,
        IArtistSearchProvider,
        IPlaylistSearchProvider,
        ISearchSuggestionProvider,
        IPlaybackProvider,
        ILyricProvider,
        IAlbumProvider,
        IArtistProvider,
        IPlaylistProvider
{
    private readonly CloudMusicApiService _api;
    private readonly OnlineMusicApiOptions _options;
    private readonly PlaybackCache _playbackCache;
    private readonly ListSnapshotCache _snapshots;

    public CloudMusicProvider(
        HttpClient httpClient,
        OnlineMusicApiOptions options,
        PlaybackCache playbackCache,
        ListSnapshotCache snapshots
    )
    {
        _api = new CloudMusicApiService(
            httpClient ?? throw new ArgumentNullException(nameof(httpClient))
        );
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _playbackCache = playbackCache ?? throw new ArgumentNullException(nameof(playbackCache));
        _snapshots = snapshots ?? throw new ArgumentNullException(nameof(snapshots));
    }

    public MusicSource Source => MusicSource.CloudMusic;

    public string DisplayName => "网易云音乐";

    public ProviderCapability Capabilities =>
        ProviderCapability.SearchSong
        | ProviderCapability.SearchAlbum
        | ProviderCapability.SearchArtist
        | ProviderCapability.SearchPlaylist
        | ProviderCapability.SearchSuggestion
        | ProviderCapability.Playback
        | ProviderCapability.Lyric
        | ProviderCapability.AlbumDetail
        | ProviderCapability.AlbumSongs
        | ProviderCapability.ArtistDetail
        | ProviderCapability.ArtistAlbums
        | ProviderCapability.PlaylistSongs;

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

        (bool isOk, CloudSearchAlbumsResponse? response) = await _api.SearchAlbumsAsync(
                keyword,
                limit,
                offset
            )
            .ConfigureAwait(false);
        ThrowIfFailed(isOk, nameof(SearchAlbumsAsync));

        List<Album> items = response?.Result?.Albums is { } albums
            ? [.. albums.Select(CloudSongMapper.FromSearchAlbum)]
            : [];

        int nextOffset = offset + items.Count;
        bool hasMore = response?.Result is { } result && nextOffset < result.AlbumCount;

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

        (bool isOk, CloudSearchArtistsResponse? response) = await _api.SearchArtistsAsync(
                keyword,
                limit,
                offset
            )
            .ConfigureAwait(false);
        ThrowIfFailed(isOk, nameof(SearchArtistsAsync));

        List<Artist> items = response?.Result?.Artists is { } artists
            ? [.. artists.Select(CloudSongMapper.FromSearchArtist)]
            : [];

        int nextOffset = offset + items.Count;
        bool hasMore = response?.Result is { } result && nextOffset < result.ArtistCount;

        return new ArtistPage
        {
            Source = Source,
            Items = items,
            Total = items.Count,
            NextOffset = hasMore ? nextOffset : null,
            HasMore = hasMore,
        };
    }

    public async Task<PlaylistPage> SearchPlaylistsAsync(
        SearchQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        string keyword = ValueNormalizer.NormalizeKeyword(query.Keyword);
        int limit = ValueNormalizer.NormalizeLimit(query.Limit);
        int offset = ValueNormalizer.NormalizeOffset(query.Offset);

        (bool isOk, CloudSearchPlaylistsResponse? response) = await _api.SearchPlaylistsAsync(
                keyword,
                limit,
                offset
            )
            .ConfigureAwait(false);
        ThrowIfFailed(isOk, nameof(SearchPlaylistsAsync));

        List<Playlist> items = response?.Result?.Playlists is { } playlists
            ? [.. playlists.Select(CloudSongMapper.FromSearchPlaylist)]
            : [];

        int nextOffset = offset + items.Count;
        bool hasMore = response?.Result is { } result && nextOffset < result.PlaylistCount;

        return new PlaylistPage
        {
            Source = Source,
            Items = items,
            Total = items.Count,
            NextOffset = hasMore ? nextOffset : null,
            HasMore = hasMore,
        };
    }

    public async Task<IReadOnlyList<SearchSuggestion>> GetSuggestionsAsync(
        string keyword,
        CancellationToken cancellationToken
    )
    {
        string normalized = ValueNormalizer.NormalizeKeyword(keyword);

        (bool isOk, CloudSearchSuggestResponse? response) = await _api.SearchSuggestionsAsync(
                normalized
            )
            .ConfigureAwait(false);
        ThrowIfFailed(isOk, nameof(GetSuggestionsAsync));

        return CloudSongMapper.FromSuggestions(response?.Result);
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

        long songId = CloudSongMapper.ParseId(id.Value);
        (bool isOk, CloudLyricResponse? response) = await _api.GetLyricAsync(songId)
            .ConfigureAwait(false);
        ThrowIfFailed(isOk, nameof(GetLyricAsync));

        if (response?.Yrc?.Lyric is { Length: > 0 } yrc)
        {
            return new Lyric
            {
                Id = id,
                Format = LyricFormat.Yrc,
                Text = yrc,
            };
        }

        if (response?.Lrc?.Lyric is { Length: > 0 } lrc)
        {
            return new Lyric
            {
                Id = id,
                Format = LyricFormat.Lrc,
                Text = lrc,
            };
        }

        return new Lyric { Id = id, Format = LyricFormat.Unknown };
    }

    public async Task<Album> GetAlbumAsync(AlbumId id, CancellationToken cancellationToken)
    {
        EnsureSource(id.Source);

        long albumId = CloudSongMapper.ParseId(id.Value);
        (bool isOk, CloudAlbumResponse? response) = await _api.GetAlbumAsync(albumId)
            .ConfigureAwait(false);
        ThrowIfFailed(isOk, nameof(GetAlbumAsync));

        CloudAlbumDetailDto album = response?.Album
            ?? throw new OnlineMusicApiResponseException(
                "网易云专辑响应缺少 album 字段。",
                nameof(GetAlbumAsync),
                Source
            );

        return CloudSongMapper.FromAlbumDetail(album, response?.Songs?.Count ?? 0);
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

        long artistId = CloudSongMapper.ParseId(id.Value);
        (bool isOk, CloudArtistAlbumResponse? response) = await _api.GetArtistAlbumsAsync(
                artistId,
                1,
                0
            )
            .ConfigureAwait(false);
        ThrowIfFailed(isOk, nameof(GetArtistAsync));

        CloudArtistDto artist = response?.Artist
            ?? throw new OnlineMusicApiResponseException(
                "网易云艺术家响应缺少 artist 字段。",
                nameof(GetArtistAsync),
                Source
            );

        (bool descriptionOk, CloudArtistDescriptionResponse? description) =
            await _api.GetArtistDescriptionAsync(artistId).ConfigureAwait(false);

        return CloudSongMapper.FromArtistSummary(
            artist,
            descriptionOk ? description?.BriefDescription : null
        );
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
        long artistId = CloudSongMapper.ParseId(id.Value);

        (bool isOk, CloudArtistAlbumResponse? response) = await _api.GetArtistAlbumsAsync(
                artistId,
                limit,
                offset
            )
            .ConfigureAwait(false);
        ThrowIfFailed(isOk, nameof(GetArtistAlbumsAsync));

        List<Album> items = response?.Albums is { } albums
            ? [.. albums.Select(album => CloudSongMapper.FromArtistAlbum(album, response.Artist))]
            : [];

        int nextOffset = offset + items.Count;
        bool hasMore = response?.Artist is { } artist && nextOffset < artist.AlbumSize;

        return new AlbumPage
        {
            Source = Source,
            Items = items,
            Total = items.Count,
            NextOffset = hasMore ? nextOffset : null,
            HasMore = hasMore,
        };
    }

    public Task<SongPage> GetPlaylistSongsAsync(
        PlaylistId id,
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
                IReadOnlyList<RawSong> snapshot = await GetPlaylistSnapshotAsync(id, token)
                    .ConfigureAwait(false);
                return Slice(snapshot, batchOffset, limit);
            },
            ProbeAndFilterAsync,
            cancellationToken
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

        (bool isOk, CloudSearchSongsResponse? response) = await _api.SearchSongsAsync(
                keyword,
                limit,
                offset
            )
            .ConfigureAwait(false);
        ThrowIfFailed(isOk, nameof(SearchSongsAsync));

        List<CloudSearchSongDto> songs = response?.Result?.Songs ?? [];
        List<RawSong> items = [with(songs.Count)];
        for (int index = 0; index < songs.Count; index++)
        {
            items.Add(new RawSong(CloudSongMapper.FromSearchSong(songs[index]), offset + index));
        }

        int nextOffset = offset + songs.Count;
        bool hasMore = response?.Result is { } result && nextOffset < result.SongCount;

        return new RawBatch(items, hasMore, nextOffset);
    }

    private async Task<MediaSource?> FetchMediaAsync(SongId id, CancellationToken cancellationToken)
    {
        long songId = CloudSongMapper.ParseId(id.Value);

        (bool isOk, CloudSongUrlResponse? response) = await _api.GetSongUrlsAsync([songId])
            .ConfigureAwait(false);
        ThrowIfFailed(isOk, nameof(ResolveMediaAsync));

        return CloudSongMapper.ToMediaSource(response?.Data?.FirstOrDefault());
    }

    private async Task<IReadOnlyList<RawSong>> ProbeAndFilterAsync(
        IReadOnlyList<RawSong> items,
        CancellationToken cancellationToken
    )
    {
        Dictionary<long, MediaSource?> mediaById = [];
        long[] ids =
        [
            .. items.Select(static item => CloudSongMapper.ParseId(item.Song.Id.Value)).Distinct(),
        ];

        foreach (long[] chunk in ids.Chunk(100))
        {
            cancellationToken.ThrowIfCancellationRequested();

            (bool isOk, CloudSongUrlResponse? response) = await _api.GetSongUrlsAsync(chunk)
                .ConfigureAwait(false);

            if (!isOk || response?.Data is null)
            {
                continue;
            }

            foreach (CloudSongUrlItem item in response.Data)
            {
                mediaById[item.Id] = CloudSongMapper.ToMediaSource(item);
            }
        }

        List<RawSong> playable = [with(items.Count)];
        foreach (RawSong raw in items)
        {
            long id = CloudSongMapper.ParseId(raw.Song.Id.Value);
            mediaById.TryGetValue(id, out MediaSource? media);
            _playbackCache.Set(raw.Song.Id, media);

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

        return playable;
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

        long albumId = CloudSongMapper.ParseId(id.Value);
        (bool isOk, CloudAlbumResponse? response) = await _api.GetAlbumAsync(albumId)
            .ConfigureAwait(false);
        ThrowIfFailed(isOk, nameof(GetAlbumSongsAsync));

        List<CloudSongTrackDto> tracks = response?.Songs ?? [];
        RawSong[] snapshot = new RawSong[tracks.Count];
        for (int index = 0; index < tracks.Count; index++)
        {
            snapshot[index] = new RawSong(CloudSongMapper.FromTrack(tracks[index]), index);
        }

        _snapshots.Set(key, snapshot);
        return snapshot;
    }

    private async Task<IReadOnlyList<RawSong>> GetPlaylistSnapshotAsync(
        PlaylistId id,
        CancellationToken cancellationToken
    )
    {
        SnapshotKey key = new(Source, SnapshotKind.PlaylistSongs, id.Value);
        if (
            _snapshots.TryGet<RawSong>(key, out IReadOnlyList<RawSong>? cached)
            && cached is not null
        )
        {
            return cached;
        }

        long playlistId = CloudSongMapper.ParseId(id.Value);
        (bool isOk, CloudPlaylistDetailResponse? response) = await _api.GetPlaylistDetailAsync(
                playlistId
            )
            .ConfigureAwait(false);
        ThrowIfFailed(isOk, nameof(GetPlaylistSongsAsync));

        List<CloudPlaylistTrackIdDto> trackIds = response?.Playlist?.TrackIds ?? [];
        Dictionary<long, CloudSongTrackDto> trackById = [];

        foreach (CloudPlaylistTrackIdDto[] chunk in trackIds.Chunk(100))
        {
            cancellationToken.ThrowIfCancellationRequested();

            (bool detailOk, CloudSongDetailResponse? detail) = await _api.GetSongDetailsAsync(
                    chunk.Select(static item => item.Id)
                )
                .ConfigureAwait(false);

            if (!detailOk || detail?.Songs is null)
            {
                continue;
            }

            foreach (CloudSongTrackDto track in detail.Songs)
            {
                trackById[track.Id] = track;
            }
        }

        List<RawSong> snapshot = [with(trackIds.Count)];
        int rawIndex = 0;
        foreach (CloudPlaylistTrackIdDto trackId in trackIds)
        {
            if (trackById.TryGetValue(trackId.Id, out CloudSongTrackDto? track))
            {
                snapshot.Add(new RawSong(CloudSongMapper.FromTrack(track), rawIndex));
                rawIndex++;
            }
        }

        _snapshots.Set(key, snapshot);
        return snapshot;
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

    private void ThrowIfFailed(bool isOk, string operation)
    {
        if (!isOk)
        {
            throw new OnlineMusicApiProviderException("网易云请求失败。", operation, Source);
        }
    }
}
