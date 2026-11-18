namespace OnlineMusicApi;

/// <summary>搜索请求。歌曲、专辑、艺术家、歌单搜索共用；<see cref="ProbeablePageQuery.ProbePlayback"/> 仅歌曲搜索有意义。</summary>
public sealed class SearchQuery : ProbeablePageQuery
{
    /// <summary>搜索关键词。</summary>
    public string Keyword { get; init; } = string.Empty;
}
