using Microsoft.EntityFrameworkCore.Design;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 设计时上下文工厂
/// </summary>
/// <remarks>
/// 给 dotnet ef 生成迁移用，每个模块派生一个空类。生成迁移不连数据库，要连时从环境变量 KMIC_DESIGN_TIME_DATABASE 读连接串
/// </remarks>
public abstract class ModuleDesignTimeFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : ModuleDbContext
{
    /// <inheritdoc />
    public TContext CreateDbContext(string[] args) =>
        StandaloneContexts.Create<TContext>(Environment.GetEnvironmentVariable("KMIC_DESIGN_TIME_DATABASE") ?? "Host=design-time;Database=design-time");
}
