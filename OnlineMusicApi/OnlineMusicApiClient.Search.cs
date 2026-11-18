using OnlineMusicApi.Internal.Providers.Abstractions;
using OnlineMusicApi.Internal.Providers.Shared;

namespace OnlineMusicApi;

public sealed partial class OnlineMusicApiClient
{
    /// <summary>搜索歌曲。返回项均来自同一个音源；开启探测时不可播放的歌曲已被剔除。</summary>
    /// <param name="source">音源。</param>
    /// <param name="query">搜索请求。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>歌曲分页结果。</returns>
    public async Task<SongPage> SearchSongsAsync(
        MusicSource source,
        SearchQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        ISongSearchProvider provider = Require<ISongSearchProvider>(
            source,
            ProviderCapability.SearchSong,
            nameof(SearchSongsAsync)
        );

        SongPage page = await provider
            .SearchSongsAsync(query, cancellationToken)
            .ConfigureAwait(false);
        PageValidator.EnsureSameSource(page, source);
        return page;
    }

    /// <summary>搜索专辑。</summary>
    /// <param name="source">音源。</param>
    /// <param name="query">搜索请求。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>专辑分页结果。</returns>
    public async Task<AlbumPage> SearchAlbumsAsync(
        MusicSource source,
        SearchQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        IAlbumSearchProvider provider = Require<IAlbumSearchProvider>(
            source,
            ProviderCapability.SearchAlbum,
            nameof(SearchAlbumsAsync)
        );

        AlbumPage page = await provider
            .SearchAlbumsAsync(query, cancellationToken)
            .ConfigureAwait(false);
        PageValidator.EnsureSameSource(page, source);
        return page;
    }

    /// <summary>搜索艺术家。</summary>
    /// <param name="source">音源。</param>
    /// <param name="query">搜索请求。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>艺术家分页结果。</returns>
    public async Task<ArtistPage> SearchArtistsAsync(
        MusicSource source,
        SearchQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        IArtistSearchProvider provider = Require<IArtistSearchProvider>(
            source,
            ProviderCapability.SearchArtist,
            nameof(SearchArtistsAsync)
        );

        ArtistPage page = await provider
            .SearchArtistsAsync(query, cancellationToken)
            .ConfigureAwait(false);
        PageValidator.EnsureSameSource(page, source);
        return page;
    }

    /// <summary>搜索歌单。</summary>
    /// <param name="source">音源。</param>
    /// <param name="query">搜索请求。</param>
    /// <param name="cancellationToken">取消标记。</param>
    /// <returns>歌单分页结果。</returns>
    public async Task<PlaylistPage> SearchPlaylistsAsync(
        MusicSource source,
        SearchQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        IPlaylistSearchProvider provider = Require<IPlaylistSearchProvider>(
            source,
            ProviderCapability.SearchPlaylist,
            nameof(SearchPlaylistsAsync)
        );

        PlaylistPage page = await provider
            .SearchPlaylistsAsync(query, cancellationToken)
            .ConfigureAwait(false);
        PageValidator.EnsureSameSource(page, source);
        return page;
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
        ISearchSuggestionProvider provider = Require<ISearchSuggestionProvider>(
            source,
            ProviderCapability.SearchSuggestion,
            nameof(GetSuggestionsAsync)
        );

        return provider.GetSuggestionsAsync(keyword, cancellationToken);
    }
}
