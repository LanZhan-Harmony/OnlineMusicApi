namespace OnlineMusicApi.Internal.Providers.FiveSing.Protocol;

internal sealed class QueryCollection : List<KeyValuePair<string, string>>
{
    public QueryCollection() { }

    public QueryCollection(int capacity)
        : base(capacity) { }

    public void Add(string key, string value) => Add(new(key, value));
}
