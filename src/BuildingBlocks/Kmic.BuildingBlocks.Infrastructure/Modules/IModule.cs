using System.Reflection;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Scheduling;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Modules;

/// <summary>模块服务端注册接口</summary>
public interface IModule
{
    /// <summary>模块名称，须与模块 Contracts 中的 ModuleInfo.Name 一致</summary>
    string Name { get; }

    /// <summary>本模块的 Application 程序集，供框架扫描用例、权限点、范围对象、编号规则、作业及字典声明</summary>
    Assembly ApplicationAssembly { get; }

    /// <summary>注册 DbContext、仓储、Contracts 实现、类型化客户端等无法自动发现的服务</summary>
    void AddServices(IServiceCollection services);

    /// <summary>声明依赖的模块</summary>
    void DeclareDependencies(IModuleDependencies dependencies)
    {
    }

    /// <summary>声明订阅的集成事件</summary>
    void ConfigureMessaging(IMessagingBuilder messaging)
    {
    }

    /// <summary>注册仅在 Worker 角色运行的后台组件（如队列拉取循环）</summary>
    void AddWorkerServices(IServiceCollection services)
    {
    }

    /// <summary>声明定时任务</summary>
    void ConfigureScheduling(ISchedulingBuilder scheduling)
    {
    }
}
