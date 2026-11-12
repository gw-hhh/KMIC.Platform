using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.Configuration.Infrastructure.Persistence;

/// <summary>
/// 设计时创建上下文，dotnet ef 生成迁移用
/// </summary>
internal sealed class ConfigurationDbContextFactory : ModuleDesignTimeFactory<ConfigurationDbContext>;
