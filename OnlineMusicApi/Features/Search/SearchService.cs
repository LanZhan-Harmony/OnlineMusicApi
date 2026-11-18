using OnlineMusicApi.Internal.Features.Shared;
using OnlineMusicApi.Internal.Providers.Abstractions;
using OnlineMusicApi.Internal.Providers.Shared;

namespace OnlineMusicApi.Internal.Features.Search;

internal sealed class SearchService(ProviderRouter router)
{
    public async Task<SongPage> SearchSongsAsync(
        MusicSource source,
        SearchQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        ISongSearchProvider provider = router.Require<ISongSearchProvider>(
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

    public async Task<AlbumPage> SearchAlbumsAsync(
        MusicSource source,
        SearchQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        IAlbumSearchProvider provider = router.Require<IAlbumSearchProvider>(
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

    public async Task<ArtistPage> SearchArtistsAsync(
        MusicSource source,
        SearchQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        IArtistSearchProvider provider = router.Require<IArtistSearchProvider>(
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

    public async Task<PlaylistPage> SearchPlaylistsAsync(
        MusicSource source,
        SearchQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        IPlaylistSearchProvider provider = router.Require<IPlaylistSearchProvider>(
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

    public Task<IReadOnlyList<SearchSuggestion>> GetSuggestionsAsync(
        MusicSource source,
        string keyword,
        CancellationToken cancellationToken
    )
    {
        ISearchSuggestionProvider provider = router.Require<ISearchSuggestionProvider>(
            source,
            ProviderCapability.SearchSuggestion,
            nameof(GetSuggestionsAsync)
        );

        return provider.GetSuggestionsAsync(keyword, cancellationToken);
    }
}
