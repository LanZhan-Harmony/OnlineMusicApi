namespace OnlineMusicApi.Internal.Providers.Bilibili.Protocol;

internal sealed class QueryCollection : List<KeyValuePair<string, string>>
{
    public QueryCollection() { }

    public QueryCollection(int capacity)
        : base(capacity) { }

    public void Add(string key, string value) => Add(new(key, value));
}
