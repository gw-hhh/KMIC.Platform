using Microsoft.EntityFrameworkCore.Design;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 设计时上下文工厂，给 dotnet ef 生成迁移用
/// </summary>
/// <remarks>
/// 每个模块派生一个空类；要连库时读环境变量 KMIC_DESIGN_TIME_DATABASE
/// </remarks>
public abstract class ModuleDesignTimeFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : ModuleDbContext
{
    /// <inheritdoc />
    public TContext CreateDbContext(string[] args) =>
        StandaloneContexts.Create<TContext>(Environment.GetEnvironmentVariable("KMIC_DESIGN_TIME_DATABASE") ?? "Host=design-time;Database=design-time");
}
