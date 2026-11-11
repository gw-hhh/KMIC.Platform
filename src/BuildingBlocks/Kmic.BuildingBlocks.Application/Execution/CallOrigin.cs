namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>
/// 请求来源
/// </summary>
public enum CallOrigin
{
    /// <summary>
    /// HTTP 请求
    /// </summary>
    Http,

    /// <summary>
    /// 其他模块通过 Contracts 调用
    /// </summary>
    ModuleCall,

    /// <summary>
    /// 集成事件处理
    /// </summary>
    Inbox,

    /// <summary>
    /// 定时任务，系统身份
    /// </summary>
    ScheduledJob,

    /// <summary>
    /// 后台作业
    /// </summary>
    BackgroundJob,

    /// <summary>
    /// 启动任务
    /// </summary>
    Startup,
}
