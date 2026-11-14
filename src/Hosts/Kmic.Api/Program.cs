using Kmic.BuildingBlocks.Application.Behaviors;
using Kmic.BuildingBlocks.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Kmic.BuildingBlocks.Presentation.OpenApi;
using Kmic.Configuration.Infrastructure;
using Kmic.Configuration.Presentation;
using Kmic.Files.Infrastructure;
using Kmic.Files.Presentation;
using Kmic.Identity.Infrastructure;
using Kmic.Identity.Presentation;
using Kmic.Notifications.Infrastructure;
using Kmic.Notifications.Presentation;
using Kmic.Operations.Infrastructure;
using Kmic.Operations.Presentation;
using Kmic.Sample.Infrastructure;
using Kmic.Sample.Presentation;
using Kmic.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 日志、追踪、指标的默认配置，要在模块注册之前
builder.AddKmicServiceDefaults();

// 模块在这里显式注册，Sample 是参考模块，只在开发与测试环境运行
builder.AddKmicHost(host => host
    .AddModule<NotificationsModule, NotificationsEndpoints>()
    .AddModule<FilesModule, FilesEndpoints>()
    .AddModule<OperationsModule, OperationsEndpoints>()
    .AddModule<ConfigurationModule, ConfigurationEndpoints>()
    .AddModule<IdentityModule, IdentityEndpoints>()
    .AddModule<SampleModule, SampleEndpoints>(onlyIn: KmicEnvironments.NonProduction));

// 选项必须是编译期常量，源码生成器要读取。数组顺序就是管道里的执行顺序
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.GenerateTypesAsInternal = true;
    options.PipelineBehaviors =
    [
        typeof(LoggingBehavior<,>),
        typeof(ExceptionTranslationBehavior<,>),
        typeof(FeatureGateBehavior<,>),
        typeof(ValidationBehavior<,>),
        typeof(AuthorizationBehavior<,>),
        typeof(ReadAuditBehavior<,>),
        typeof(IdempotencyBehavior<,>),
        typeof(TransactionBehavior<,>),
    ];
});

// XML 注释只在调用 AddOpenApi 的项目里读取，所以由宿主调用
builder.Services.AddOpenApi("v1", options => options.AddKmicConventions());

var app = builder.Build();
// 请求摘要日志在最前面，错误处理之前，500 也能记到
app.UseKmicRequestLogging();
app.UseKmicHost();

// 接口文档与界面只在开发和测试环境开放
if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

await app.RunAsync();

/// <summary>
/// 给 WebApplicationFactory 用的入口类型
/// </summary>
public partial class Program;
