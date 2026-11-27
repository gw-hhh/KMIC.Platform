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

// 配置默认日志、追踪与指标，须在模块注册之前调用
builder.AddKmicServiceDefaults();

// 显式注册模块；Sample 为参考模块，仅在非生产环境注册
builder.AddKmicHost(host => host
    .AddModule<NotificationsModule, NotificationsEndpoints>()
    .AddModule<FilesModule, FilesEndpoints>()
    .AddModule<OperationsModule, OperationsEndpoints>()
    .AddModule<ConfigurationModule, ConfigurationEndpoints>()
    .AddModule<IdentityModule, IdentityEndpoints>()
    .AddModule<SampleModule, SampleEndpoints>(onlyIn: KmicEnvironments.NonProduction));

// 选项须为编译期常量以便源码生成器读取；数组顺序即为请求管道的执行顺序
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

// XML 文档注释仅在调用 AddOpenApi 的程序集内被读取，因此由宿主统一注册
builder.Services.AddOpenApi("v1", options => options.AddKmicConventions());

var app = builder.Build();
// 请求摘要日志须置于最外层（错误处理中间件之前），确保 500 异常也能被记录
app.UseKmicRequestLogging();
app.UseKmicHost();

// 接口文档与界面仅在非生产环境开放
if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

await app.RunAsync();

/// <summary>供集成测试 WebApplicationFactory 引用的宿主入口类型</summary>
public partial class Program;
