namespace Kmic.BuildingBlocks.Application.Execution;

public enum CallOrigin
{
    Http,

    /// <summary>其他模块通过 Contracts 调用。</summary>
    ModuleCall,

    /// <summary>集成事件处理。</summary>
    Inbox,

    /// <summary>定时任务（以系统身份执行）。</summary>
    ScheduledJob,

    BackgroundJob,

    Startup,
}
