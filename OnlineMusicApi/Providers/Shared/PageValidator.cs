namespace OnlineMusicApi.Internal.Providers.Shared;

internal static class PageValidator
{
    public static void EnsureSameSource<T>(Page<T> page, MusicSource expectedSource)
    {
        ArgumentNullException.ThrowIfNull(page);

        if (page.Source != expectedSource)
        {
            throw new OnlineMusicApiResponseException(
                $"页面音源 {page.Source} 与预期 {expectedSource} 不一致。",
                "PageValidation",
                expectedSource
            );
        }

        if (!typeof(ISourcedEntity).IsAssignableFrom(typeof(T)))
        {
            return;
        }

        foreach (T item in page.Items)
        {
            if (item is ISourcedEntity entity && entity.Source != expectedSource)
            {
                throw new OnlineMusicApiResponseException(
                    $"页面数据混入了其他音源：{entity.Source}。",
                    "PageValidation",
                    expectedSource
                );
            }
        }
    }
}
