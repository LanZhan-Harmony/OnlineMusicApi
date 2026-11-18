using System.Collections.Concurrent;

namespace OnlineMusicApi.Internal.Infrastructure.Caching;

internal sealed class PlaybackCache
{
    private readonly TimeSpan _lifetime;
    private readonly Lock _lock = new();
    private readonly Dictionary<SongId, Entry> _cache = [];
    private readonly ConcurrentDictionary<SongId, Lazy<Task<MediaSource?>>> _pending = new();

    public PlaybackCache(TimeSpan lifetime)
    {
        _lifetime = lifetime;
    }

    public bool TryGet(SongId id, out MediaSource? media)
    {
        media = null;
        if (_lifetime <= TimeSpan.Zero)
        {
            return false;
        }

        lock (_lock)
        {
            if (!_cache.TryGetValue(id, out Entry entry))
            {
                return false;
            }

            if (entry.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                _cache.Remove(id);
                return false;
            }

            media = entry.Media;
            return true;
        }
    }

    public async Task<MediaSource?> GetOrAddAsync(
        SongId id,
        Func<CancellationToken, Task<MediaSource?>> factory,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (TryGet(id, out MediaSource? cached))
        {
            return cached;
        }

        if (_lifetime <= TimeSpan.Zero)
        {
            return await factory(cancellationToken).ConfigureAwait(false);
        }

        Lazy<Task<MediaSource?>> pending = _pending.GetOrAdd(id, _ => new Lazy<Task<MediaSource?>>(
            () => factory(CancellationToken.None),
            LazyThreadSafetyMode.ExecutionAndPublication
        ));

        try
        {
            MediaSource? media = await pending
                .Value.WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            Set(id, media);
            return media;
        }
        finally
        {
            _pending.TryRemove(new KeyValuePair<SongId, Lazy<Task<MediaSource?>>>(id, pending));
        }
    }

    public void Set(SongId id, MediaSource? media)
    {
        if (_lifetime <= TimeSpan.Zero)
        {
            return;
        }

        lock (_lock)
        {
            _cache[id] = new Entry(media, DateTimeOffset.UtcNow + _lifetime);
        }
    }

    private readonly record struct Entry(MediaSource? Media, DateTimeOffset ExpiresAt);
}
