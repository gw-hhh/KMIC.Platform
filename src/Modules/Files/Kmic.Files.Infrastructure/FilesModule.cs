using System.Reflection;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Application;
using Kmic.Files.Contracts;
using Kmic.Files.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.Files.Infrastructure;

/// <summary>
/// Files 模块的服务端注册类
/// </summary>
/// <remarks>
/// 处理器、校验器、EF 配置自动发现；这里只写自动发现做不到的注册
/// </remarks>
public sealed class FilesModule : IModule
{
    /// <inheritdoc />
    public string Name => FilesModuleInfo.Name;

    /// <inheritdoc />
    public Assembly ApplicationAssembly => typeof(IFilesQueryDb).Assembly;

    /// <inheritdoc />
    public void AddServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddModuleDbContexts<FilesDbContext, FilesQueryDbContext, IFilesQueryDb>();
    }
}
