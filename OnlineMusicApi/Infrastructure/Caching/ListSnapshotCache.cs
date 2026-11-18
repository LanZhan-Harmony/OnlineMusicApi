namespace OnlineMusicApi.Internal.Infrastructure.Caching;

internal enum SnapshotKind
{
    AlbumSongs,
    PlaylistSongs,
    ArtistAlbums,
}

internal readonly record struct SnapshotKey(MusicSource Source, SnapshotKind Kind, string Id);

internal sealed class ListSnapshotCache
{
    private readonly TimeSpan _lifetime;
    private readonly int _maxTotalSongs;
    private readonly Lock _lock = new();
    private readonly Dictionary<SnapshotKey, SnapshotEntry> _entries = [];
    private int _totalSongs;

    public ListSnapshotCache(TimeSpan lifetime, int maxTotalSongs)
    {
        _lifetime = lifetime;
        _maxTotalSongs = maxTotalSongs;
    }

    public bool TryGet<T>(SnapshotKey key, out IReadOnlyList<T>? items)
    {
        items = null;

        if (_lifetime <= TimeSpan.Zero)
        {
            return false;
        }

        lock (_lock)
        {
            if (!_entries.TryGetValue(key, out SnapshotEntry entry))
            {
                return false;
            }

            if (entry.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                _entries.Remove(key);
                _totalSongs -= entry.Count;
                return false;
            }

            items = (IReadOnlyList<T>)entry.Items;
            return true;
        }
    }

    public void Set<T>(SnapshotKey key, IReadOnlyList<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (_lifetime <= TimeSpan.Zero || items.Count == 0)
        {
            return;
        }

        lock (_lock)
        {
            if (_entries.Remove(key, out SnapshotEntry old))
            {
                _totalSongs -= old.Count;
            }

            _entries[key] = new SnapshotEntry(
                items,
                items.Count,
                DateTimeOffset.UtcNow + _lifetime
            );
            _totalSongs += items.Count;

            while (_totalSongs > _maxTotalSongs && _entries.Count > 0)
            {
                KeyValuePair<SnapshotKey, SnapshotEntry> oldest = _entries.MinBy(static pair =>
                    pair.Value.ExpiresAt
                );

                _entries.Remove(oldest.Key);
                _totalSongs -= oldest.Value.Count;
            }
        }
    }

    public void Remove(SnapshotKey key)
    {
        lock (_lock)
        {
            if (_entries.Remove(key, out SnapshotEntry entry))
            {
                _totalSongs -= entry.Count;
            }
        }
    }

    private sealed record SnapshotEntry(object Items, int Count, DateTimeOffset ExpiresAt);
}
