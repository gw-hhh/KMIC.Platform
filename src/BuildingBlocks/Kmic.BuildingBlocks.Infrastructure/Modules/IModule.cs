using System.Reflection;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Scheduling;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Modules;

/// <summary>
/// 模块的服务端注册类
/// </summary>
/// <remarks>
/// AddServices 里只写自动发现做不到的注册
/// </remarks>
public interface IModule
{
    /// <summary>
    /// 模块名，与模块 Contracts 里的 ModuleInfo.Name 一致
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 本模块的 Application 程序集，框架从这里扫描用例、权限点、范围对象、编号规则、作业、字典类型等声明
    /// </summary>
    Assembly ApplicationAssembly { get; }

    /// <summary>
    /// 注册 DbContext、仓储、Contracts 实现、类型化客户端和部署配置
    /// </summary>
    void AddServices(IServiceCollection services);

    /// <summary>
    /// 声明依赖的模块
    /// </summary>
    void DeclareDependencies(IModuleDependencies dependencies)
    {
    }

    /// <summary>
    /// 声明订阅的集成事件
    /// </summary>
    void ConfigureMessaging(IMessagingBuilder messaging)
    {
    }

    /// <summary>
    /// 注册只在 Worker 角色上运行的后台组件，如领取队列的循环
    /// </summary>
    void AddWorkerServices(IServiceCollection services)
    {
    }

    /// <summary>
    /// 声明定时任务
    /// </summary>
    void ConfigureScheduling(ISchedulingBuilder scheduling)
    {
    }
}
