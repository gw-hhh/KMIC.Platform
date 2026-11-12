using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.Configuration.Infrastructure;
using Kmic.Identity.Infrastructure;
using Kmic.MigrationRunner;
using Kmic.Sample.Infrastructure;
using Microsoft.Extensions.Hosting;

// 环境取 DOTNET_ENVIRONMENT，默认生产；配置来自 appsettings、用户机密（开发）和环境变量
var builder = Host.CreateApplicationBuilder();

// 模块清单和 Kmic.Api 的 Program.cs 保持一致；Sample 是参考模块，生产环境不迁移
var modules = new ToolModules()
    .Add<ConfigurationModule>()
    .Add<IdentityModule>()
    .Add<SampleModule>(nonProductionOnly: true);

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellation.Cancel();
};

return await RunnerApp.RunAsync(args, builder.Configuration, modules, builder.Environment.IsProduction(), Console.Out, TimeProvider.System, cancellation.Token);
