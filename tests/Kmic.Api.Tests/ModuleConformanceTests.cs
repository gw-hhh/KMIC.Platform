using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.Api.Tests;

/// <summary>
/// 宿主里注册的每个模块都要满足的约束：新增模块后自动覆盖，不需要各模块各写一份
/// </summary>
/// <remarks>
/// 模块注册、用例目录、权限点目录、EF 模型校验（含唯一索引都登记了错误码）在宿主启动时检查，启动失败就是这里的测试失败
/// </remarks>
public sealed class ModuleConformanceTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    [Fact]
    public async Task Host_StartsWithEveryRegisteredModule()
    {
        using var client = ClientFor();

        var ready = await client.GetAsync("/health/ready", Ct);

        ready.EnsureSuccessStatusCode();
    }

    [Fact]
    public void EveryModuleModel_MatchesItsLatestMigration()
    {
        var changed = ModuleMigrator.ModulesWithPendingModelChanges(Factory.Services, ConnectionString);

        changed.ShouldBeEmpty("这些模块改了实体模型但没有生成迁移：" + string.Join("、", changed));
    }

    [Fact]
    public async Task EveryModuleHasNoPendingMigrationsInTheTestDatabase()
    {
        var pending = await ModuleMigrator.PendingAsync(Factory.Services, ConnectionString, Ct);

        pending.ShouldBeEmpty();
    }
}
