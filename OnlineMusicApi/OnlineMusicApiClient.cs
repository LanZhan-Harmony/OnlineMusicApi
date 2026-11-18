using OnlineMusicApi.Internal.Infrastructure.Runtime;
using OnlineMusicApi.Internal.Providers.Abstractions;

namespace OnlineMusicApi;

/// <summary>多源在线音乐客户端。所有实例方法显式区分音源。</summary>
public sealed partial class OnlineMusicApiClient : IDisposable
{
    private readonly OnlineMusicClientRuntime _runtime;

    /// <summary>使用默认 HttpClient 创建客户端。</summary>
    /// <param name="options">客户端配置。</param>
    public OnlineMusicApiClient(OnlineMusicApiOptions? options = null)
        : this(new OnlineMusicClientRuntime(null, options)) { }

    /// <summary>使用外部 HttpClient 创建客户端。外部 HttpClient 不会被本客户端释放。</summary>
    /// <param name="httpClient">外部 HttpClient。</param>
    /// <param name="options">客户端配置。</param>
    public OnlineMusicApiClient(HttpClient httpClient, OnlineMusicApiOptions? options = null)
        : this(new OnlineMusicClientRuntime(httpClient, options)) { }

    private OnlineMusicApiClient(OnlineMusicClientRuntime runtime)
    {
        _runtime = runtime;
    }

    /// <summary>已注册音源的能力信息。</summary>
    public ProviderRegistry Providers => _runtime.Registry;

    /// <summary>释放客户端。外部注入的 HttpClient 不会被释放。</summary>
    public void Dispose()
    {
        _runtime.Dispose();
    }

    private T Require<T>(MusicSource source, ProviderCapability capability, string operation)
        where T : class, IMusicProvider
    {
        if (
            !_runtime.Registry.Supports(source, capability)
            || !_runtime.Providers.TryGetValue(source, out IMusicProvider? provider)
            || provider is not T typed
        )
        {
            throw new OnlineMusicApiNotSupportedException(operation, source, capability);
        }

        return typed;
    }
}
