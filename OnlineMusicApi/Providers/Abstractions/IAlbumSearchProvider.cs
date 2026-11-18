namespace OnlineMusicApi.Internal.Providers.Abstractions;

internal interface IAlbumSearchProvider : IMusicProvider
{
    Task<AlbumPage> SearchAlbumsAsync(SearchQuery query, CancellationToken cancellationToken);
}
