namespace OnlineMusicApi.Internal.Providers.Shared;

internal readonly record struct RawSong(Song Song, int RawIndex);

internal sealed record RawBatch(IReadOnlyList<RawSong> Items, bool HasMore, int NextOffset);

internal static class PageAssembler
{
    public static async Task<SongPage> AssembleSongsAsync(
        MusicSource source,
        int requestOffset,
        int limit,
        bool probe,
        bool greedy,
        int maxGreedyPages,
        Func<int, CancellationToken, Task<RawBatch>> fetchBatch,
        Func<IReadOnlyList<RawSong>, CancellationToken, Task<IReadOnlyList<RawSong>>> probeAndFilter,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(fetchBatch);
        ArgumentNullException.ThrowIfNull(probeAndFilter);

        List<RawSong> playable = [];
        int consumedOffset = requestOffset;
        int rawCount = 0;
        int pages = 0;
        bool hasMore = false;

        while (true)
        {
            RawBatch batch = await fetchBatch(consumedOffset, cancellationToken).ConfigureAwait(false);
            pages++;
            rawCount += batch.Items.Count;

            if (batch.Items.Count > 0)
            {
                IReadOnlyList<RawSong> filtered = probe
                    ? await probeAndFilter(batch.Items, cancellationToken).ConfigureAwait(false)
                    : batch.Items;

                playable.AddRange(filtered);
            }

            consumedOffset = batch.NextOffset;
            hasMore = batch.HasMore;

            if (!greedy
                || playable.Count >= limit
                || !hasMore
                || pages >= maxGreedyPages)
            {
                break;
            }
        }

        int? nextOffset;
        if (!hasMore)
        {
            nextOffset = null;
        }
        else if (greedy && playable.Count >= limit)
        {
            nextOffset = playable[limit - 1].RawIndex + 1;
        }
        else
        {
            nextOffset = consumedOffset;
        }

        if (playable.Count > limit)
        {
            playable.RemoveRange(limit, playable.Count - limit);
        }

        int consumedRawCount = greedy && playable.Count >= limit
            ? playable[limit - 1].RawIndex - requestOffset + 1
            : rawCount;
        int returnedCount = Math.Min(playable.Count, limit);
        int excluded = Math.Max(consumedRawCount - returnedCount, 0);
        string? warning = excluded > 0 ? $"已过滤 {excluded} 首不可播放歌曲" : null;
        if (greedy && playable.Count < limit && hasMore && pages >= maxGreedyPages)
        {
            warning = warning is null
                ? $"已连续拉取 {pages} 页仍未凑满 {limit} 首"
                : $"{warning}；已连续拉取 {pages} 页仍未凑满 {limit} 首";
        }

        return new SongPage
        {
            Source = source,
            Items = [.. playable.Select(static item => item.Song)],
            Total = playable.Count,
            NextOffset = nextOffset,
            HasMore = hasMore,
            Warning = warning,
        };
    }
}