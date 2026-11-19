using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kmic.Api.Tests;

/// <summary>
/// 真实宿主（Kmic.Api）+ 每个测试类自己的数据库 + 测试用的访问上下文加载器
/// </summary>
/// <remarks>
/// 默认只有 Api 角色，要后台处理的测试另配 Worker；身份默认用 <see cref="Loader"/> 里登记的测试身份，realIdentity 为 true 时用 Identity 模块的真实登录。
/// 同一个测试类共用一个实例
/// </remarks>
public sealed class ApiFactory(
    string connectionString,
    bool realIdentity = false,
    IReadOnlyDictionary<string, string>? extraSettings = null,
    Action<IServiceCollection>? configureServices = null) : WebApplicationFactory<Program>
{
    /// <summary>
    /// 签发令牌用的密钥，宿主用它验证令牌
    /// </summary>
    public TokenFactory Tokens { get; } = new();

    /// <summary>
    /// 登记身份用的加载器
    /// </summary>
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

        // 放宽未登录请求的限额，避免同一个类里前面测试的登录让后面的被限流（测试客户端没有来源地址，都落在同一个分区）
        // 限流本身由 Presentation 的测试覆盖
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
