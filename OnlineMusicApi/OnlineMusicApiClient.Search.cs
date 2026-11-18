namespace OnlineMusicApi;

public sealed partial class OnlineMusicApiClient
{
    /// <summary>搜索歌曲。返回项均来自同一个音源；开启探测时不可播放的歌曲已被剔除。</summary>
    /// <param name="source">音源。</param>
    /// <param name="query">搜索请求。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>歌曲分页结果。</returns>
    public Task<SongPage> SearchSongsAsync(
        MusicSource source,
        SearchQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return _search.SearchSongsAsync(source, query, cancellationToken);
    }

    /// <summary>搜索专辑。</summary>
    /// <param name="source">音源。</param>
    /// <param name="query">搜索请求。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>专辑分页结果。</returns>
    public Task<AlbumPage> SearchAlbumsAsync(
        MusicSource source,
        SearchQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return _search.SearchAlbumsAsync(source, query, cancellationToken);
    }

    /// <summary>搜索艺术家。</summary>
    /// <param name="source">音源。</param>
    /// <param name="query">搜索请求。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>艺术家分页结果。</returns>
    public Task<ArtistPage> SearchArtistsAsync(
        MusicSource source,
        SearchQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return _search.SearchArtistsAsync(source, query, cancellationToken);
    }

    /// <summary>搜索歌单。</summary>
    /// <param name="source">音源。</param>
    /// <param name="query">搜索请求。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>歌单分页结果。</returns>
    public Task<PlaylistPage> SearchPlaylistsAsync(
        MusicSource source,
        SearchQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return _search.SearchPlaylistsAsync(source, query, cancellationToken);
    }

    /// <summary>获取搜索联想。仅部分音源支持。</summary>
    /// <param name="source">音源。</param>
    /// <param name="keyword">关键词。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>联想项列表，只含类型与标题。</returns>
    public Task<IReadOnlyList<SearchSuggestion>> GetSuggestionsAsync(
        MusicSource source,
        string keyword,
        CancellationToken cancellationToken = default
    )
    {
        return _search.GetSuggestionsAsync(source, keyword, cancellationToken);
    }
}
