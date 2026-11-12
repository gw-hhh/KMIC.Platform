using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.Sample.Infrastructure.Persistence;

/// <summary>
/// 设计时创建上下文，dotnet ef 生成迁移用
/// </summary>
internal sealed class SampleDbContextFactory : ModuleDesignTimeFactory<SampleDbContext>;
