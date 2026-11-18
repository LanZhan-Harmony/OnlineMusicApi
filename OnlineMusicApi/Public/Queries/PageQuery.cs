namespace OnlineMusicApi;

/// <summary>分页请求基类。</summary>
public class PageQuery
{
    /// <summary>起始偏移量，从 0 开始。</summary>
    public int Offset { get; init; }

    /// <summary>本页最大条数，取值 1~50。</summary>
    public int Limit { get; init; } = 20;

    /// <summary>忽略列表快照缓存重新拉取；对 ServerPaged 源无影响。</summary>
    public bool Refresh { get; init; }
}
