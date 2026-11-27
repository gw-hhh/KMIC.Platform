using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Presentation.Authentication;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

// 内存测试宿主：接口层 + 假发送器 + 测试认证，不连数据库
internal sealed class ApiHost : IAsyncDisposable
{
    private ApiHost(WebApplication app, SenderScript script)
    {
        App = app;
        Script = script;
    }

    public WebApplication App { get; }

    public SenderScript Script { get; }

    public TestAccessContextLoader Loader => App.Services.GetRequiredService<TestAccessContextLoader>();

    public HttpClient Client { get; private set; } = null!;

    public HttpClient Anonymous { get; private set; } = null!;

    // 构建但不启动宿主，用于验证启动校验失败
    public static ApiHost Create(ModuleEndpoints[] modules, Type[]? messages = null, string environment = "Production", Action<IServiceCollection>? services = null, Action<WebApplication>? beforeEndpoints = null, bool skipValidation = false, IReadOnlyDictionary<string, string?>? configuration = null, bool jwt = false)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment, ContentRootPath = AppContext.BaseDirectory });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(configuration ?? new Dictionary<string, string?>(StringComparer.Ordinal));

        var script = new SenderScript();
        if (jwt)
        {
            // 真正的令牌验证与访问上下文加载；身份由测试登记在 TestAccessContextLoader 里
            builder.Services.AddKmicAuthentication();
            builder.Services.AddScoped<ExecutionScope>();
            builder.Services.AddScoped<IExecutionContext>(sp => sp.GetRequiredService<ExecutionScope>());
            builder.Services.AddSingleton<TestAccessContextLoader>();
            builder.Services.AddSingleton<IAccessContextLoader>(sp => sp.GetRequiredService<TestAccessContextLoader>());
        }
        else
        {
            builder.Services.AddAuthentication(TestAuthHandler.SchemeName).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
            builder.Services.AddAuthorization();
        }

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
        app.UseRouting();
        app.UseAuthentication();
        if (jwt)
        {
            app.UseKmicAccessContext();
        }

        app.UseKmicLimits();
        app.UseAuthorization();
        beforeEndpoints?.Invoke(app);
        app.MapKmicEndpoints();
        return new ApiHost(app, script);
    }

    public static async Task<ApiHost> StartAsync(ModuleEndpoints[] modules, Type[]? messages = null, string environment = "Production", Action<IServiceCollection>? services = null, Action<WebApplication>? beforeEndpoints = null, bool skipValidation = false, IReadOnlyDictionary<string, string?>? configuration = null, bool jwt = false)
    {
        var host = Create(modules, messages, environment, services, beforeEndpoints, skipValidation, configuration, jwt);
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

    public HttpClient ClientFor(string user)
    {
        var client = App.GetTestClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.Header, user);
        return client;
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        Anonymous?.Dispose();
        await App.DisposeAsync();
    }
}
