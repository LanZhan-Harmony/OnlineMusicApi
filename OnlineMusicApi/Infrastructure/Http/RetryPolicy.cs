namespace OnlineMusicApi.Internal.Infrastructure.Http;

internal static class RetryPolicy
{
    public static async Task<T> ExecuteAsync<T>(
        Func<int, CancellationToken, Task<T>> operation,
        int attempts,
        Func<Exception, bool> isTransient,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(isTransient);

        attempts = Math.Clamp(attempts, 1, 8);
        Exception? last = null;

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await operation(attempt, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
                when (attempt + 1 < attempts
                    && (
                        isTransient(exception)
                        || (
                            exception is OperationCanceledException
                            && !cancellationToken.IsCancellationRequested
                        )
                    )
                )
            {
                last = exception;
            }

            TimeSpan delay = TimeSpan.FromMilliseconds(250 * (attempt + 1));
            if (
                last is OnlineMusicApiException { RetryAfter: { } retryAfter }
                && retryAfter > TimeSpan.Zero
            )
            {
                delay = retryAfter + TimeSpan.FromMilliseconds(Random.Shared.Next(300, 700));
            }

            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }

        throw last ?? new InvalidOperationException("重试策略异常：没有可抛出的异常。");
    }

    public static bool IsTransient(Exception exception)
    {
        return exception
            is HttpRequestException
                or TimeoutException
                or OnlineMusicApiTransportException
                or OnlineMusicApiHttpException { IsTransient: true }
                or OnlineMusicApiProviderException { IsTransient: true };
    }
}
