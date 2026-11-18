namespace OnlineMusicApi;

/// <summary>搜索联想项。只含类型与标题，不含 ID。</summary>
public sealed class SearchSuggestion
{
    /// <summary>联想项类型。</summary>
    public SearchSuggestionKind Kind { get; init; } = SearchSuggestionKind.Unknown;

    /// <summary>联想标题。</summary>
    public string Title { get; init; } = string.Empty;
}
