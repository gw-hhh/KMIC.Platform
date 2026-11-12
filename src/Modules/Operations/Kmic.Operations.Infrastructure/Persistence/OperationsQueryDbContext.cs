using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Operations.Application;
using Kmic.Operations.Contracts;
using Kmic.Operations.Domain.Audit;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Operations.Infrastructure.Persistence;

/// <summary>
/// Operations 模块的查询上下文：不跟踪实体，查询处理器通过 IOperationsQueryDb 使用
/// </summary>
internal sealed class OperationsQueryDbContext(DbContextOptions<OperationsQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), IOperationsQueryDb
{
    /// <inheritdoc />
    public override string Module => OperationsModuleInfo.Name;

    /// <inheritdoc />
    public IQueryable<AuditLog> AuditLogs => Set<AuditLog>();

    /// <inheritdoc />
    public IQueryable<AuditLog> WithTarget(IQueryable<AuditLog> audits, string targetsJson)
    {
        ArgumentNullException.ThrowIfNull(audits);

        return audits.Where(a => EF.Functions.JsonContains(a.Targets, targetsJson));
    }

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInOperationsIdConverters();

    /// <inheritdoc />
    protected override void ConfigureModel(ModelBuilder modelBuilder) => OperationsModel.Configure(modelBuilder);
}
