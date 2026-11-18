namespace OnlineMusicApi;

/// <summary>已注册音源的只读能力信息。</summary>
public sealed class ProviderRegistry
{
    private readonly Dictionary<MusicSource, ProviderCapability> _capabilities;

    internal ProviderRegistry(IEnumerable<ProviderRegistration> registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);

        _capabilities = [];
        foreach (ProviderRegistration registration in registrations)
        {
            _capabilities[registration.Source] = registration.Capabilities;
        }

        SupportedSources = [.. _capabilities.Keys.OrderBy(static source => (int)source)];
    }

    /// <summary>已注册的音源列表，按枚举值排序。</summary>
    public IReadOnlyList<MusicSource> SupportedSources { get; }

    /// <summary>判断音源是否支持指定能力。</summary>
    /// <param name="source">音源。</param>
    /// <param name="capability">能力。</param>
    /// <returns>支持返回 true。</returns>
    public bool Supports(MusicSource source, ProviderCapability capability)
    {
        return _capabilities.TryGetValue(source, out ProviderCapability capabilities)
            && (capabilities & capability) == capability;
    }

    /// <summary>获取音源支持的全部能力。</summary>
    /// <param name="source">音源。</param>
    /// <returns>能力集合；未注册的音源返回 <see cref="ProviderCapability.None"/>。</returns>
    public ProviderCapability GetCapabilities(MusicSource source)
    {
        return _capabilities.GetValueOrDefault(source, ProviderCapability.None);
    }
}

/// <summary>Provider 注册项。</summary>
internal readonly record struct ProviderRegistration(
    MusicSource Source,
    ProviderCapability Capabilities
);
