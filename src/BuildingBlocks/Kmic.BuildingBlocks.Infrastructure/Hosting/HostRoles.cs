namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

/// <summary>
/// 实例承担的角色，同一个镜像按配置 Kmic:Roles 决定
/// </summary>
[Flags]
public enum HostRoles
{
    /// <summary>
    /// 没有角色，不能启动
    /// </summary>
    None = 0,

    /// <summary>
    /// 接 HTTP 请求
    /// </summary>
    Api = 1,

    /// <summary>
    /// 投递和处理消息、执行后台作业和定时任务
    /// </summary>
    Worker = 2,
}
