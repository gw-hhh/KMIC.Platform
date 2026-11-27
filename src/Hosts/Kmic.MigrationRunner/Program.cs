using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.Configuration.Infrastructure;
using Kmic.Files.Infrastructure;
using Kmic.Identity.Infrastructure;
using Kmic.MigrationRunner;
using Kmic.Notifications.Infrastructure;
using Kmic.Operations.Infrastructure;
using Kmic.Sample.Infrastructure;
using Microsoft.Extensions.Hosting;

// 环境名称取自 DOTNET_ENVIRONMENT（默认 Production）；配置依次读取 appsettings、开发用户机密与环境变量
var builder = Host.CreateApplicationBuilder();

// 模块清单与 Kmic.Api 的 Program.cs 保持一致；Sample 为参考模块，不在生产环境执行迁移
var modules = new ToolModules()
    .Add<NotificationsModule>()
    .Add<FilesModule>()
    .Add<OperationsModule>()
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
