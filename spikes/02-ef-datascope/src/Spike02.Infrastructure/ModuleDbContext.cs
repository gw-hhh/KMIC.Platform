using Microsoft.EntityFrameworkCore;
using Spike02.Domain;

namespace Spike02.Infrastructure;

/// <summary>
/// 模块上下文基类：模块配置完成后，为声明了数据范围的实体注册 DataScope 命名过滤器
/// </summary>
public abstract class ModuleDbContext(DbContextOptions options, IDataScope dataScope) : DbContext(options)
{
    public const string DataScopeFilter = "DataScope";

    // 过滤器表达式经这些成员读取当前请求的范围（05-05 第 5.4 节）
    internal bool DataScopeUnrestricted => dataScope.Unrestricted;

    internal UserId? DataScopeCurrentUser => dataScope.CurrentUser;

    internal EffectiveScope DataScopeFor(string scopeObject) => dataScope.For(scopeObject);

    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureModel(modelBuilder);
        DataScopeFilters.Apply(modelBuilder, this);
    }

    protected abstract void ConfigureModel(ModelBuilder modelBuilder);
}
