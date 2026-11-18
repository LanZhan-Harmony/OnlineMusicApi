using System.Net;
using OnlineMusicApi.Internal.Infrastructure.Caching;
using OnlineMusicApi.Internal.Infrastructure.Http;
using OnlineMusicApi.Internal.Providers.Abstractions;
using OnlineMusicApi.Internal.Providers.Bilibili;
using OnlineMusicApi.Internal.Providers.CloudMusic;
using OnlineMusicApi.Internal.Providers.FiveSing;
using OnlineMusicApi.Internal.Providers.KuwoMusic;
using OnlineMusicApi.Internal.Providers.QQMusic;

namespace OnlineMusicApi.Internal.Infrastructure.Runtime;

internal sealed class OnlineMusicClientRuntime : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly Dictionary<MusicSource, IMusicProvider> _providers;

    public OnlineMusicClientRuntime(HttpClient? httpClient, OnlineMusicApiOptions? options)
    {
        _httpClient = httpClient ?? CreateDefaultHttpClient();
        _ownsHttpClient = httpClient is null;
        Options = (options ?? new OnlineMusicApiOptions()).Normalize();
        Transport = new HttpTransport(_httpClient, Options);
        PlaybackCache = new PlaybackCache(Options.PlaybackCacheLifetime);
        ListSnapshots = new ListSnapshotCache(
            Options.ListSnapshotLifetime,
            Options.ListSnapshotMaxTotalSongs
        );

        CloudMusicProvider cloudMusic = new(_httpClient, Options, PlaybackCache, ListSnapshots);
        QQMusicProvider qqMusic = new(_httpClient, Options, PlaybackCache, ListSnapshots);
        KuwoMusicProvider kuwoMusic = new(_httpClient, Options, PlaybackCache, ListSnapshots);
        BilibiliProvider bilibili = new(_httpClient, Options, PlaybackCache, ListSnapshots);
        FiveSingProvider fiveSing = new(_httpClient, Options, PlaybackCache, ListSnapshots);
        _providers = new Dictionary<MusicSource, IMusicProvider>
        {
            [MusicSource.CloudMusic] = cloudMusic,
            [MusicSource.QQMusic] = qqMusic,
            [MusicSource.KuwoMusic] = kuwoMusic,
            [MusicSource.Bilibili] = bilibili,
            [MusicSource.FiveSing] = fiveSing,
        };

        Registry = new ProviderRegistry(
            _providers.Values.Select(static provider => new ProviderRegistration(
                provider.Source,
                provider.Capabilities
            ))
        );
    }

    public OnlineMusicApiOptions Options { get; }

    public HttpTransport Transport { get; }

    public PlaybackCache PlaybackCache { get; }

    public ListSnapshotCache ListSnapshots { get; }

    public IReadOnlyDictionary<MusicSource, IMusicProvider> Providers => _providers;

    public ProviderRegistry Registry { get; }

    public static HttpClient CreateDefaultHttpClient()
    {
        SocketsHttpHandler handler = new()
        {
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            EnableMultipleHttp2Connections = true,
            UseCookies = false,
        };

        return new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
