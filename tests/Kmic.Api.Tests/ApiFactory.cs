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
/// 角色只有 Api：投递、作业、调度这些后台处理另有测试。Identity 模块还没有，
/// 身份由测试登记在 <see cref="Loader"/> 里，令牌用测试自己的密钥签发
/// </remarks>
public sealed class ApiFactory(string connectionString, TokenFactory tokens, bool realIdentity = false, IReadOnlyDictionary<string, string>? extraSettings = null) : WebApplicationFactory<Program>
{
    /// <summary>
    /// 登记身份用的加载器
    /// </summary>
    public TestAccessContextLoader Loader => Services.GetRequiredService<TestAccessContextLoader>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Testing");
        builder.UseSetting("Kmic:Roles", "Api");
        builder.UseSetting("Kmic:Database:ConnectionString", connectionString);
        builder.UseSetting("Kmic:Database:CheckQueryVisibility", "true");
        foreach (var (key, value) in tokens.Settings())
        {
            builder.UseSetting(key, value);
        }

        foreach (var (key, value) in extraSettings ?? new Dictionary<string, string>(StringComparer.Ordinal))
        {
            builder.UseSetting(key, value);
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
