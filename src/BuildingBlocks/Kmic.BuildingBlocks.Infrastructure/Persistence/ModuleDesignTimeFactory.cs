using Microsoft.EntityFrameworkCore.Design;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>EF Core 迁移设计时上下文工厂（派生空实现即可；需连库时设置环境变量 KMIC_DESIGN_TIME_DATABASE）</summary>
public abstract class ModuleDesignTimeFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : ModuleDbContext
{
    public TContext CreateDbContext(string[] args) =>
        StandaloneContexts.Create<TContext>(Environment.GetEnvironmentVariable("KMIC_DESIGN_TIME_DATABASE") ?? "Host=design-time;Database=design-time");
}
