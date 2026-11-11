using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

/// <summary>
/// 内存里的测试宿主：接口层 + 假发送器 + 测试认证
/// </summary>
/// <remarks>
/// 不连数据库、不跑真正的处理器，只检验接口层自己的行为
/// </remarks>
internal sealed class ApiHost : IAsyncDisposable
{
    private ApiHost(WebApplication app, SenderScript script)
    {
        App = app;
        Script = script;
    }

    public WebApplication App { get; }

    public SenderScript Script { get; }

    /// <summary>
    /// 已登录的客户端
    /// </summary>
    public HttpClient Client { get; private set; } = null!;

    /// <summary>
    /// 没有登录的客户端
    /// </summary>
    public HttpClient Anonymous { get; private set; } = null!;

    /// <summary>
    /// 建好但不启动：校验失败的测试要看启动时抛什么
    /// </summary>
    public static ApiHost Create(ModuleEndpoints[] modules, Type[]? messages = null, string environment = "Production", Action<IServiceCollection>? services = null, Action<WebApplication>? beforeEndpoints = null, bool skipValidation = false)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment, ContentRootPath = AppContext.BaseDirectory });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        var script = new SenderScript();
        builder.Services.AddAuthentication(TestAuthHandler.SchemeName).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddKmicPresentation();
        builder.Services.AddSingleton(UseCaseCatalog.Build(messages ?? Messages.All));
        builder.Services.AddScoped<RequestMetadata>();
        builder.Services.AddSingleton(script);
        builder.Services.AddScoped<FakeSender>();
        builder.Services.AddScoped<ICommandSender>(sp => sp.GetRequiredService<FakeSender>());
        builder.Services.AddScoped<IQuerySender>(sp => sp.GetRequiredService<FakeSender>());
        foreach (var module in modules)
        {
            builder.Services.AddSingleton(module);
        }

        services?.Invoke(builder.Services);
        if (skipValidation)
        {
            builder.Services.Remove(builder.Services.Single(d => d.ImplementationType == typeof(EndpointValidationFilter)));
        }

        var app = builder.Build();
        app.UseKmicErrorHandling();
        app.UseAuthentication();
        app.UseAuthorization();
        beforeEndpoints?.Invoke(app);
        app.MapKmicEndpoints();
        return new ApiHost(app, script);
    }

    public static async Task<ApiHost> StartAsync(ModuleEndpoints[] modules, Type[]? messages = null, string environment = "Production", Action<IServiceCollection>? services = null, Action<WebApplication>? beforeEndpoints = null, bool skipValidation = false)
    {
        var host = Create(modules, messages, environment, services, beforeEndpoints, skipValidation);
        await host.StartAsync();
        return host;
    }

    public async Task StartAsync()
    {
        await App.StartAsync(TestContext.Current.CancellationToken);
        Anonymous = App.GetTestClient();
        Client = App.GetTestClient();
        Client.DefaultRequestHeaders.Add(TestAuthHandler.Header, "alice");
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        Anonymous?.Dispose();
        await App.DisposeAsync();
    }
}
