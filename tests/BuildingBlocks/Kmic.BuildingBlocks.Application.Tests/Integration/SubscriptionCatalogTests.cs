using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Tests.Fixtures;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Auditing;
using Shouldly;
using static Kmic.BuildingBlocks.Application.Tests.Integration.IntegrationMessageSamples;

namespace Kmic.BuildingBlocks.Application.Tests.Integration;

public sealed class SubscriptionCatalogTests
{
    // 测试程序集推出的模块名是 BuildingBlocks；框架消息 AuditRecorded 属于 Framework，可以订阅
    private const string Module = "BuildingBlocks";

    private static readonly UseCaseCatalog _useCases = UseCaseCatalog.Build([typeof(SyncThingCommand), typeof(ChangePasswordCommand)]);
    private static readonly IntegrationMessageCatalog _messages = IntegrationMessageCatalog.Build([typeof(AuditRecorded), typeof(ThingChanged)]);

    [Fact]
    public void Build_ValidSubscription_MapsMessageToCommand()
    {
        var module = new ModuleSubscriptions(Module);
        module.Subscribe<AuditRecorded, SyncThingCommand>("BuildingBlocks.CollectAudit", (_, _) => new SyncThingCommand(), o => o.Ordered().RebuildWith<IThingSnapshotSource, ThingChanged, ThingRow>(item => new ThingRow(item.Snapshot.Id)));

        var subscription = SubscriptionCatalog.Build([module], _useCases, _messages).Find("BuildingBlocks.CollectAudit", 1).ShouldNotBeNull();

        subscription.Message.Name.ShouldBe("Framework.AuditRecorded");
        subscription.CommandType.ShouldBe(typeof(SyncThingCommand));
        subscription.IsOrdered.ShouldBeTrue();
        var rebuild = subscription.Rebuild.ShouldNotBeNull();
        rebuild.Source.ShouldBe(typeof(IThingSnapshotSource));
        rebuild.ReadModel.ShouldBe(typeof(ThingRow));
    }

    [Fact]
    public void Build_WrongDeclarations_ListsEveryProblem()
    {
        var module = new ModuleSubscriptions(Module);
        module.Subscribe<AuditRecorded, SyncThingCommand>("Sales.CollectAudit", (_, _) => new SyncThingCommand());
        module.Subscribe<ThingChanged, SyncThingCommand>("BuildingBlocks.OwnEvent", (_, _) => new SyncThingCommand());
        module.Subscribe<AuditRecorded, ChangePasswordCommand>("BuildingBlocks.NotSystem", (_, _) => new ChangePasswordCommand());
        module.Subscribe<Unnamed, SyncThingCommand>("BuildingBlocks.Unknown", (_, _) => new SyncThingCommand());
        module.Subscribe<AuditRecorded, SyncThingCommand>("BuildingBlocks.Twice", (_, _) => new SyncThingCommand());
        module.Subscribe<AuditRecorded, SyncThingCommand>("BuildingBlocks.Twice", (_, _) => new SyncThingCommand());
        module.Subscribe<AuditRecorded, SyncThingCommand>("BuildingBlocks.StillUsed", (_, _) => new SyncThingCommand());
        module.Retired("BuildingBlocks.StillUsed", "已改用新订阅");

        var thrown = Should.Throw<DeclarationException>(() => SubscriptionCatalog.Build([module], _useCases, _messages));

        thrown.Problems.ShouldContain(p => p.Contains("Sales.CollectAudit", StringComparison.Ordinal) && p.Contains("BuildingBlocks.", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.OwnEvent", StringComparison.Ordinal) && p.Contains("领域事件", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.NotSystem", StringComparison.Ordinal) && p.Contains("[SystemOnly]", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.Unknown", StringComparison.Ordinal) && p.Contains("目录", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.Twice", StringComparison.Ordinal) && p.Contains("两次", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.StillUsed", StringComparison.Ordinal) && p.Contains("退役", StringComparison.Ordinal));
        thrown.Problems.Count.ShouldBe(6);
    }

    [Fact]
    public void Build_OtherModuleCommand_Rejected()
    {
        var module = new ModuleSubscriptions("Sales");
        module.Subscribe<AuditRecorded, SyncThingCommand>("Sales.CollectAudit", (_, _) => new SyncThingCommand());

        var thrown = Should.Throw<DeclarationException>(() => SubscriptionCatalog.Build([module], _useCases, _messages));

        thrown.Problems.ShouldHaveSingleItem().ShouldContain("Sales 模块的命令");
    }

    [Fact]
    public void Build_ReadModelOfOtherModule_Rejected()
    {
        var module = new ModuleSubscriptions("Sales");
        module.Subscribe<AuditRecorded, SyncThingCommand>(
            "Sales.Mirror", (_, _) => new SyncThingCommand(), o => o.RebuildWith<IThingSnapshotSource, ThingChanged, ThingRow>(item => new ThingRow(item.Snapshot.Id)));

        var thrown = Should.Throw<DeclarationException>(() => SubscriptionCatalog.Build([module], _useCases, _messages));

        thrown.Problems.ShouldContain(p => p.Contains("ThingRow", StringComparison.Ordinal) && p.Contains("Sales 模块", StringComparison.Ordinal));
    }

    [Fact]
    public void Build_VersionsWithDifferentRebuild_Rejected()
    {
        var messages = IntegrationMessageCatalog.Build([typeof(ThingChanged), typeof(ThingChangedV2)]);
        var module = new ModuleSubscriptions("Sales");
        module.Subscribe<ThingChanged, SyncThingCommand>(
            "Sales.Mirror", (_, _) => new SyncThingCommand(), o => o.RebuildWith<IThingSnapshotSource, ThingChanged, ThingRow>(item => new ThingRow(item.Snapshot.Id)));
        module.Subscribe<ThingChangedV2, SyncThingCommand>(
            "Sales.Mirror", (_, _) => new SyncThingCommand(), o => o.RebuildWith<IOtherSnapshotSource, ThingChangedV2, ThingRow>(item => new ThingRow(item.Snapshot.Id)));

        var thrown = Should.Throw<DeclarationException>(() => SubscriptionCatalog.Build([module], _useCases, messages));

        thrown.Problems.ShouldContain(p => p.Contains("Sales.Mirror", StringComparison.Ordinal) && p.Contains("不同的重建方式", StringComparison.Ordinal));
    }
}
