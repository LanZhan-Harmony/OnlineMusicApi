namespace OnlineMusicApi.Internal.Providers.Abstractions;

internal interface ISearchSuggestionProvider : IMusicProvider
{
    Task<IReadOnlyList<SearchSuggestion>> GetSuggestionsAsync(
        string keyword,
        CancellationToken cancellationToken
    );
}
