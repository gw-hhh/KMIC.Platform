using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.Files.Infrastructure.Persistence;

/// <summary>
/// 设计时创建上下文，dotnet ef 生成迁移用
/// </summary>
internal sealed class FilesDbContextFactory : ModuleDesignTimeFactory<FilesDbContext>;
