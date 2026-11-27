namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

/// <summary>实例运行角色（通过 Kmic:Roles 配置）</summary>
[Flags]
public enum HostRoles
{
    /// <summary>未指定角色（禁止启动）</summary>
    None = 0,

    /// <summary>处理 HTTP 请求</summary>
    Api = 1,

    /// <summary>消息投递与消费、后台作业与定时调度</summary>
    Worker = 2,
}
