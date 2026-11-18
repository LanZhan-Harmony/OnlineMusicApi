namespace OnlineMusicApi;

/// <summary>可单独覆盖播放探测开关的分页请求基类。</summary>
public class ProbeablePageQuery : PageQuery
{
    /// <summary>仅本次调用覆盖全局探测开关；null 表示跟随 <see cref="OnlineMusicApiOptions.EnablePlaybackProbe"/>。</summary>
    public bool? ProbePlayback { get; init; }
}
