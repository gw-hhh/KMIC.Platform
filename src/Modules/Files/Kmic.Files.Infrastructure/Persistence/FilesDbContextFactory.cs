using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.Files.Infrastructure.Persistence;

// 设计时数据库上下文工厂，供 dotnet ef 生成迁移使用
internal sealed class FilesDbContextFactory : ModuleDesignTimeFactory<FilesDbContext>;
