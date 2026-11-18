namespace OnlineMusicApi.Internal.Providers.Abstractions;

internal interface ISongSearchProvider : IMusicProvider
{
    Task<SongPage> SearchSongsAsync(SearchQuery query, CancellationToken cancellationToken);
}
