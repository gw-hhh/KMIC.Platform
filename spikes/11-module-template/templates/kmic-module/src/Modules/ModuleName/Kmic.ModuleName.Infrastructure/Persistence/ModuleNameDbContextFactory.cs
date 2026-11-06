using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kmic.ModuleName.Infrastructure.Persistence;

/// <summary>
/// 生成迁移时创建上下文，不连接数据库
/// </summary>
internal sealed class ModuleNameDbContextFactory : IDesignTimeDbContextFactory<ModuleNameDbContext>
{
    public ModuleNameDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<ModuleNameDbContext>().UseNpgsql("Host=localhost").Options);
}
