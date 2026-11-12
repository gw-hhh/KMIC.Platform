using System.Reflection;
using System.Text.Json.Nodes;
using Kmic.BuildingBlocks.Contracts.Auditing;
using Kmic.BuildingBlocks.Testing.Contracts;
using Shouldly;

namespace Kmic.BuildingBlocks.Testing.Tests.Contracts;

/// <summary>
/// 用框架自己的契约（审计记录）验证快照检查
/// </summary>
public sealed class MessageContractSnapshotsTests : IDisposable
{
    private const string SnapshotFile = "Framework.AuditRecorded.v1.json";

    private static readonly Assembly[] _contracts = [typeof(AuditRecorded).Assembly];

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"kmic-snapshots-{Guid.CreateVersion7():N}");

    private string SnapshotPath => Path.Combine(_directory, SnapshotFile);

    [Fact]
    public void NoSnapshotYet_ReportsEachMessage()
    {
        var problems = MessageContractSnapshots.Check(_contracts, _directory, update: false);

        problems.ShouldContain(p => p.StartsWith(SnapshotFile, StringComparison.Ordinal) && p.Contains("没有快照", StringComparison.Ordinal));
    }

    [Fact]
    public void UpdateMode_WritesSnapshots_AndTheNextCheckPasses()
    {
        MessageContractSnapshots.Check(_contracts, _directory, update: true).ShouldBeEmpty();

        File.Exists(SnapshotPath).ShouldBeTrue();
        MessageContractSnapshots.Check(_contracts, _directory, update: false).ShouldBeEmpty();
    }

    [Fact]
    public void FieldAddedAsRequired_IsBreaking()
    {
        // 快照里没有 module，相当于代码后来新增了一个必填字段：旧消息里没有它，读不出来
        EditSnapshot(schema =>
        {
            ((JsonObject)schema["properties"]!).Remove("module");
            var required = (JsonArray)schema["required"]!;
            required.Remove(required.Single(r => string.Equals(r!.GetValue<string>(), "module", StringComparison.Ordinal)));
        });

        var problem = MessageContractSnapshots.Check(_contracts, _directory, update: false).ShouldHaveSingleItem();

        problem.ShouldContain("破坏性");
        problem.ShouldContain("module 变成了必填");
    }

    [Fact]
    public void RemovedField_IsBreaking()
    {
        // 快照里有 legacyField，代码里已经没有了
        EditSnapshot(schema => ((JsonObject)schema["properties"]!).Add("legacyField", new JsonObject { ["type"] = "string" }));

        var problem = MessageContractSnapshots.Check(_contracts, _directory, update: false).ShouldHaveSingleItem();

        problem.ShouldContain("破坏性");
        problem.ShouldContain("legacyField 被删除");
    }

    [Fact]
    public void ChangedType_IsBreaking()
    {
        EditSnapshot(schema => schema["properties"]!["module"]!["type"] = "integer");

        MessageContractSnapshots.Check(_contracts, _directory, update: false).ShouldHaveSingleItem().ShouldContain("module 类型变了");
    }

    [Fact]
    public void OtherDifferences_AreCompatibleAndAskForANewSnapshot()
    {
        EditSnapshot(schema => schema["properties"]!["module"]!["description"] = "只是说明变了");

        var problem = MessageContractSnapshots.Check(_contracts, _directory, update: false).ShouldHaveSingleItem();

        problem.ShouldContain("兼容");
        problem.ShouldNotContain("破坏性");
    }

    [Fact]
    public void SnapshotWithoutAMessage_IsReportedAsAnOrphan_AndRemovedByUpdate()
    {
        MessageContractSnapshots.Check(_contracts, _directory, update: true);
        var orphan = Path.Combine(_directory, "Gone.Message.v1.json");
        File.WriteAllText(orphan, "{}");

        MessageContractSnapshots.Check(_contracts, _directory, update: false).ShouldHaveSingleItem().ShouldContain("Gone.Message.v1.json");

        MessageContractSnapshots.Check(_contracts, _directory, update: true).ShouldBeEmpty();
        File.Exists(orphan).ShouldBeFalse();
    }

    [Fact]
    public void FindContractAssemblies_FollowsKmicReferencesFromTheEntryAssembly()
    {
        var found = MessageContractSnapshots.FindContractAssemblies(typeof(MessageContractSnapshotsTests).Assembly);

        found.ShouldContain(typeof(AuditRecorded).Assembly);
        found.ShouldAllBe(a => a.GetName().Name!.EndsWith(".Contracts", StringComparison.Ordinal));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    // 先生成当前快照，再改动快照文件，模拟“代码相对快照发生了变化”
    private void EditSnapshot(Action<JsonObject> change)
    {
        MessageContractSnapshots.Check(_contracts, _directory, update: true);
        var schema = (JsonObject)JsonNode.Parse(File.ReadAllText(SnapshotPath))!;
        change(schema);
        File.WriteAllText(SnapshotPath, schema.ToJsonString());
    }
}
