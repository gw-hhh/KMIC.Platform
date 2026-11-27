using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.Api.Tests;

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

        changed.ShouldBeEmpty("以下模块修改了实体模型但尚未生成迁移：" + string.Join("、", changed));
    }

    [Fact]
    public async Task EveryModuleHasNoPendingMigrationsInTheTestDatabase()
    {
        var pending = await ModuleMigrator.PendingAsync(Factory.Services, ConnectionString, Ct);

        pending.ShouldBeEmpty();
    }
}
