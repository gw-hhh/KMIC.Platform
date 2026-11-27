using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.Configuration.Infrastructure.Persistence;

// 供 dotnet ef 生成迁移使用
internal sealed class ConfigurationDbContextFactory : ModuleDesignTimeFactory<ConfigurationDbContext>;
