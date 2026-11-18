using OnlineMusicApi.Internal.Providers.Abstractions;

namespace OnlineMusicApi.Internal.Features.Shared;

internal sealed class ProviderRouter
{
    private readonly IReadOnlyDictionary<MusicSource, IMusicProvider> _providers;
    private readonly ProviderRegistry _registry;

    public ProviderRouter(
        IReadOnlyDictionary<MusicSource, IMusicProvider> providers,
        ProviderRegistry registry
    )
    {
        _providers = providers;
        _registry = registry;
    }

    public T Require<T>(MusicSource source, ProviderCapability capability, string operation)
        where T : class, IMusicProvider
    {
        if (
            !_registry.Supports(source, capability)
            || !_providers.TryGetValue(source, out IMusicProvider? provider)
            || provider is not T typed
        )
        {
            throw new OnlineMusicApiNotSupportedException(operation, source, capability);
        }

        return typed;
    }
}
