namespace OnlineMusicApi.Internal.Providers.Abstractions;

internal interface IArtistSearchProvider : IMusicProvider
{
    Task<ArtistPage> SearchArtistsAsync(SearchQuery query, CancellationToken cancellationToken);
}
