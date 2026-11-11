using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Tests.Persistence.InvalidModel;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Security;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 构建模型时执行模型校验，一次列出全部问题；不需要数据库
/// </summary>
public sealed class ModelValidationTests
{
    [Fact]
    public void InvalidModel_RejectedWithEveryProblemListed()
    {
        var options = new DbContextOptionsBuilder<InvalidModelDbContext>().UseNpgsql("Host=unused").Options;

        // 上下文构造时就要用到模型（变更跟踪器），模型有误时解析上下文即失败
        var error = Should.Throw<DeclarationException>(() => new InvalidModelDbContext(options, Services()).Dispose());

        error.Problems.ShouldSatisfyAllConditions(
            p => p.ShouldContain(m => m.StartsWith("Gadget.Price：decimal 必须声明精度", StringComparison.Ordinal)),
            p => p.ShouldContain(m => m.StartsWith("Gadget.Ratio：不能用 float / double", StringComparison.Ordinal)),
            p => p.ShouldContain(m => m.StartsWith("Gadget.MadeAt：不能用 DateTime", StringComparison.Ordinal)),
            p => p.ShouldContain(m => m.StartsWith("Gadget.Name：字符串必须声明最大长度", StringComparison.Ordinal)),
            p => p.ShouldContain(m => m.StartsWith("Gadget：表名 Gadgets 必须与实体类名 Gadget 相同", StringComparison.Ordinal)),
            p => p.ShouldContain(m => m.StartsWith("Gadget：有所属人或所属组织，必须声明 HasDataScope", StringComparison.Ordinal)),
            p => p.ShouldContain(m => m.StartsWith("Gadget：唯一索引（Name）要用 MapsToError 声明", StringComparison.Ordinal)),
            p => p.ShouldContain(m => m.StartsWith("Gadget.Widget：聚合之间只存 ID", StringComparison.Ordinal)),
            p => p.ShouldContain(m => m.StartsWith("GadgetPart.Gadget：导航只能从父实体指向子实体", StringComparison.Ordinal)),
            p => p.ShouldContain(m => m.StartsWith("GadgetPart：只有聚合根能实现 ISoftDeletable", StringComparison.Ordinal)),
            p => p.ShouldContain(m => m.StartsWith("Widget：映射到 schema Elsewhere", StringComparison.Ordinal)),
            p => p.ShouldContain(m => m.StartsWith("Widget：只有实现 IHasOwner 或 IHasOrgUnit 的聚合根能声明 HasDataScope", StringComparison.Ordinal)),
            p => p.ShouldContain(m => m.StartsWith("Widget：范围对象 Probe.Unknown 没有声明", StringComparison.Ordinal)),
            p => p.ShouldContain(m => m.StartsWith("Sprocket：范围对象 Other.Thing 属于其他模块", StringComparison.Ordinal)));
    }

    [Fact]
    public void StringOnlyMappings_OnNonStringProperty_Throw()
    {
        var builder = new ModelBuilder().Entity<Ticket>();

        Should.Throw<InvalidOperationException>(() => builder.Property(t => t.Amount).IsUnboundedText());
        Should.Throw<InvalidOperationException>(() => builder.Property(t => t.Amount).IsCaseInsensitive());
    }

    [Fact]
    public void MapsToError_OnNonUniqueIndex_Throws()
    {
        var builder = new ModelBuilder().Entity<Ticket>();

        Should.Throw<InvalidOperationException>(() => builder.HasIndex(t => t.Title).MapsToError(TicketErrors.NumberDuplicated));
    }

    private static ModuleDbContextServices Services()
    {
        var execution = new ExecutionScope();
        execution.Initialize(CallOrigin.Startup, CurrentUser.System());
        return new ModuleDbContextServices(
            execution,
            new UnrestrictedScope(),
            DataScopeObjectCatalog.Build([typeof(ProbeScopes), typeof(OtherScopes)]),
            TimeProvider.System,
            new SaveChangesGate(),
            new RecordingSecurityEventWriter());
    }

    private sealed class UnrestrictedScope : IDataScope
    {
        public bool Unrestricted => true;

        public EffectiveScope For(string scopeObject) => EffectiveScope.Unrestricted;
    }
}
