namespace OnlineMusicApi.Internal.Providers.Abstractions;

internal interface IMusicProvider
{
    MusicSource Source { get; }

    string DisplayName { get; }

    ProviderCapability Capabilities { get; }
}
