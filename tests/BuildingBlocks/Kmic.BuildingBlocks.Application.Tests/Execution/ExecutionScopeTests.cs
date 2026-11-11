using Kmic.BuildingBlocks.Application.Execution;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Execution;

public sealed class ExecutionScopeTests
{
    [Fact]
    public void EntryOrigin_DuringModuleCall_StaysAsEntrySet()
    {
        var scope = new ExecutionScope();
        scope.Initialize(CallOrigin.BackgroundJob, CurrentUser.System());

        using (ExecutionScope.EnterModuleCall())
        {
            // 数据源按入口来源选择：后台作业中的 Contracts 调用仍用后台连接池
            scope.Origin.ShouldBe(CallOrigin.ModuleCall);
            scope.EntryOrigin.ShouldBe(CallOrigin.BackgroundJob);
        }

        scope.Origin.ShouldBe(CallOrigin.BackgroundJob);
    }

    [Fact]
    public void Initialize_Twice_Throws()
    {
        var scope = new ExecutionScope();
        scope.Initialize(CallOrigin.Http, CurrentUser.Anonymous);

        Should.Throw<InvalidOperationException>(() => scope.Initialize(CallOrigin.Inbox, CurrentUser.System()));
    }
}
