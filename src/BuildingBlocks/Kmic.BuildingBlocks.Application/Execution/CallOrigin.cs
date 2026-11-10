namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>
/// 调用来源，决定授权语义、幂等是否生效以及使用哪个数据源
/// </summary>
public enum CallOrigin
{
    /// <summary>
    /// HTTP 请求，身份为令牌中的用户与会话的当前角色
    /// </summary>
    Http,

    /// <summary>
    /// 其他模块经 Contracts 查询调用，身份沿用调用方
    /// </summary>
    ModuleCall,

    /// <summary>
    /// 处理集成事件，系统身份
    /// </summary>
    Inbox,

    /// <summary>
    /// 定时任务，系统身份
    /// </summary>
    ScheduledJob,

    /// <summary>
    /// 后台作业，身份为提交作业的用户
    /// </summary>
    BackgroundJob,

    /// <summary>
    /// 启动任务，系统身份
    /// </summary>
    Startup,
}
