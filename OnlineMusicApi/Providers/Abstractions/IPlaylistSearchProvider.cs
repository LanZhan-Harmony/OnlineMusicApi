namespace OnlineMusicApi.Internal.Providers.Abstractions;

internal interface IPlaylistSearchProvider : IMusicProvider
{
    Task<PlaylistPage> SearchPlaylistsAsync(SearchQuery query, CancellationToken cancellationToken);
}
