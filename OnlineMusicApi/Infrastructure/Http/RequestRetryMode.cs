namespace OnlineMusicApi.Internal.Infrastructure.Http;

internal enum RequestRetryMode
{
    ReadOnly,
    IdempotentWrite,
    NoRetry,
}
