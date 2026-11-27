using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kmic.Api.Tests;

/// <summary>真实 Kmic.Api 宿主工厂，默认仅启用 Api 角色与测试身份加载器（realIdentity=true 时启用真实登录）</summary>
public sealed class ApiFactory(
    string connectionString,
    bool realIdentity = false,
    IReadOnlyDictionary<string, string>? extraSettings = null,
    Action<IServiceCollection>? configureServices = null) : WebApplicationFactory<Program>
{
    public TokenFactory Tokens { get; } = new();

    public TestAccessContextLoader Loader => Services.GetRequiredService<TestAccessContextLoader>();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        Tokens.Dispose();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Testing");
        builder.UseSetting("Kmic:Roles", "Api");
        builder.UseSetting("Kmic:Database:ConnectionString", connectionString);
        builder.UseSetting("Kmic:Database:CheckQueryVisibility", "true");

        // 放宽匿名请求限额，避免同一测试类内前置测试的登录消耗后续测试的配额（测试客户端无来源地址，均落入同一分区）
        // 限流逻辑本身由 Presentation 测试覆盖
        builder.UseSetting("Kmic:Http:AnonymousRequestsPerMinute", "100000");
        foreach (var (key, value) in Tokens.Settings())
        {
            builder.UseSetting(key, value);
        }

        foreach (var (key, value) in extraSettings ?? new Dictionary<string, string>(StringComparer.Ordinal))
        {
            builder.UseSetting(key, value);
        }

        if (configureServices is not null)
        {
            builder.ConfigureTestServices(configureServices);
        }

        if (realIdentity)
        {
            return;
        }

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<TestAccessContextLoader>();
            services.RemoveAll<IAccessContextLoader>();
            services.AddSingleton<IAccessContextLoader>(sp => sp.GetRequiredService<TestAccessContextLoader>());
        });
    }
}
