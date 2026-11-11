using Microsoft.EntityFrameworkCore.Design;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// dotnet ef 生成迁移时创建模块写入上下文的工厂；每个模块派生一个空类即可
/// </summary>
/// <remarks>
/// 生成迁移不连接数据库；需要连接时（如 dotnet ef migrations script --idempotent 之外的命令）从环境变量 KMIC_DESIGN_TIME_DATABASE 读取
/// </remarks>
public abstract class ModuleDesignTimeFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : ModuleDbContext
{
    /// <inheritdoc />
    public TContext CreateDbContext(string[] args) =>
        StandaloneContexts.Create<TContext>(Environment.GetEnvironmentVariable("KMIC_DESIGN_TIME_DATABASE") ?? "Host=design-time;Database=design-time");
}
