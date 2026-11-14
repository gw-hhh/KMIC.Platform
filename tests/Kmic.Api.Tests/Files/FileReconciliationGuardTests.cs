using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Files.Application.Cleanup;
using Kmic.Files.Domain.Errors;
using Shouldly;

namespace Kmic.Api.Tests.Files;

/// <summary>
/// 对账发现的差异超过上限时整个放弃，什么也不删
/// </summary>
public sealed class FileReconciliationGuardTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : FilesApiTestBase(
        database,
        template,
        storageRoot: _root,
        moreSettings: new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Files:Storage:Environment"] = Environment,
            ["Files:ReconcileMaxChanges"] = "2",
        }),
    IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Environment = "guard";

    private static readonly string _root = Path.Combine(Path.GetTempPath(), "kmic-reconcile-guard-" + Guid.CreateVersion7().ToString("N"));

    [Fact]
    public async Task TooManyDifferences_NothingIsDeleted_AndTheRunFails()
    {
        var keys = new List<string>();
        for (var i = 0; i < 3; i++)
        {
            var at = TimeProvider.System.GetUtcNow().AddDays(-5);
            var key = FileStorageKeys.For(Environment, Guid.CreateVersion7(at), at);
            var path = Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllTextAsync(path, "内容", Ct);
            keys.Add(path);
        }

        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.ScheduledJob);
        var result = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new ReconcileStorageCommand(TimeProvider.System.GetUtcNow()), Ct);

        result.ShouldBeFailure(FilesErrors.ReconcileSuspicious);
        keys.ShouldAllBe(path => File.Exists(path));
    }
}
