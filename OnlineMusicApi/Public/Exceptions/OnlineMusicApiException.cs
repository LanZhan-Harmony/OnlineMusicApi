namespace OnlineMusicApi;

/// <summary>本库所有异常的基类，携带用于诊断与重试决策的结构化信息。</summary>
public class OnlineMusicApiException : Exception
{
    /// <summary>初始化异常。</summary>
    /// <param name="message">面向调用方的错误信息，不得包含敏感数据。</param>
    /// <param name="operation">失败的操作名称。</param>
    /// <param name="source">所属音源。</param>
    /// <param name="requestUri">关联的请求地址。</param>
    /// <param name="statusCode">HTTP 状态码。</param>
    /// <param name="providerCode">上游业务错误码。</param>
    /// <param name="isTransient">是否可安全重试。</param>
    /// <param name="retryAfter">上游建议的重试等待时间。</param>
    /// <param name="innerException">内部异常。</param>
    public OnlineMusicApiException(
        string message,
        string operation,
        MusicSource source,
        Uri? requestUri = null,
        int? statusCode = null,
        string? providerCode = null,
        bool isTransient = false,
        TimeSpan? retryAfter = null,
        Exception? innerException = null
    )
        : base(message, innerException)
    {
        Operation = operation;
        Source = source;
        RequestUri = requestUri;
        StatusCode = statusCode;
        ProviderCode = providerCode;
        IsTransient = isTransient;
        RetryAfter = retryAfter;
    }

    /// <summary>失败的操作名称。</summary>
    public string Operation { get; }

    /// <summary>所属音源。</summary>
    public new MusicSource Source { get; }

    /// <summary>关联的请求地址。</summary>
    public Uri? RequestUri { get; }

    /// <summary>HTTP 状态码。</summary>
    public int? StatusCode { get; }

    /// <summary>上游业务错误码。</summary>
    public string? ProviderCode { get; }

    /// <summary>是否可安全重试。</summary>
    public bool IsTransient { get; }

    /// <summary>上游建议的重试等待时间。</summary>
    public TimeSpan? RetryAfter { get; }
}
