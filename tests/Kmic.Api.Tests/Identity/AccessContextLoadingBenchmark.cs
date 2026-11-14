using System.Diagnostics;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Operations.Application;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 访问上下文加载耗时的参考测量：在较大的数据量上反复加载，输出分位数
/// </summary>
/// <remarks>
/// 设计的目标是状态查询一次不到 2 毫秒。这里测的是整个加载（状态查询加缓存查找），数据在本机 Docker 的 PostgreSQL 上，
/// 只作参考，不替代测试环境的压测。平时不跑（Explicit），要测的时候：
/// dotnet test --project tests/Kmic.Api.Tests --filter-class "*AccessContextLoadingBenchmark" --explicit on
/// </remarks>
public sealed class AccessContextLoadingBenchmark(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const int Users = 20_000;
    private const int Samples = 20_000;
    private const int Roles = 8;

    private IAccessContextLoader Loader => Factory.Services.GetRequiredService<IAccessContextLoader>();

    [Fact(Explicit = true)]
    public async Task LoadForRequest_OnTwentyThousandUsers_ReportsPercentiles()
    {
        var roles = new List<Guid>();
        for (var i = 0; i < Roles; i++)
        {
            roles.Add(await Seed.RoleAsync($"压测角色{i}", [OperationsPermissions.AuditView, OperationsPermissions.JobsView, OperationsPermissions.MessagesView]));
        }

        await Seed.SqlAsync(
            """
            INSERT INTO "Identity"."User" ("Id", "Account", "DisplayName", "Kind", "Status", "PasswordChangeRequired", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
            SELECT gen_random_uuid(), 'bench-' || n, '压测用户' || n, 'Normal', 'Active', false, now(), '00000000-0000-0000-0000-000000000001', 0
            FROM generate_series(1, @count) AS n
            """,
            ("count", Users));
        await Seed.SqlAsync(
            """
            INSERT INTO "Identity"."UserRoleGrant" ("Id", "RoleId", "GrantedBy", "GrantedAt", "CreatedAt", "CreatedBy", "UserId")
            SELECT gen_random_uuid(), (@roles::uuid[])[1 + (row_number() OVER ())::int % @roleCount], '00000000-0000-0000-0000-000000000001', now(), now(), '00000000-0000-0000-0000-000000000001', u."Id"
            FROM "Identity"."User" u WHERE u."Account" LIKE 'bench-%'
            """,
            ("roles", roles.ToArray()), ("roleCount", Roles));
        await Seed.SqlAsync(
            """
            INSERT INTO "Identity"."Session" ("Id", "UserId", "CurrentRoleId", "ClientType", "Status", "LastRefreshedAt", "RefreshExpiresAt", "AbsoluteExpiresAt", "ImpersonationAllowsWrite", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
            SELECT gen_random_uuid(), g."UserId", g."RoleId", 'App', 'Active', now(), now() + interval '7 days', now() + interval '30 days', false, now(), g."UserId", 0
            FROM "Identity"."UserRoleGrant" g JOIN "Identity"."User" u ON u."Id" = g."UserId" WHERE u."Account" LIKE 'bench-%'
            """);
        var rows = await ReadSessionsAsync();
        rows.Count.ShouldBe(Users);

        // 先跑一轮预热：加载器的缓存、连接池、查询计划
        foreach (var row in rows.Take(500))
        {
            (await Loader.LoadForRequestAsync(UserId.From(row.User), SessionId.From(row.Session), RoleId.From(row.Role), Ct)).Status.ShouldBe(AccessLoadStatus.Loaded);
        }

        var sequential = await MeasureAsync(rows, parallelism: 1);
        var concurrent = await MeasureAsync(rows, parallelism: 16);

        // 测试输出不一定显示，同时写一份到临时目录
        var report = $"访问上下文加载，{Users} 个用户、{Samples} 次：顺序 {sequential}；16 路并发 {concurrent}";
        TestContext.Current.SendDiagnosticMessage(report);
        await File.WriteAllTextAsync(Path.Combine(Path.GetTempPath(), "kmic-access-benchmark.txt"), report, Ct);
    }

    private async Task<List<(Guid User, Guid Session, Guid Role)>> ReadSessionsAsync()
    {
        var rows = new List<(Guid, Guid, Guid)>();
        await using var data = Npgsql.NpgsqlDataSource.Create(ConnectionString);
        await using var command = data.CreateCommand("""SELECT s."UserId", s."Id", s."CurrentRoleId" FROM "Identity"."Session" s JOIN "Identity"."User" u ON u."Id" = s."UserId" WHERE u."Account" LIKE 'bench-%'""");
        await using var reader = await command.ExecuteReaderAsync(Ct);
        while (await reader.ReadAsync(Ct))
        {
            rows.Add((reader.GetGuid(0), reader.GetGuid(1), reader.GetGuid(2)));
        }

        return rows;
    }

    private async Task<string> MeasureAsync(List<(Guid User, Guid Session, Guid Role)> rows, int parallelism)
    {
        var random = new Random(20261113);
        var picks = Enumerable.Range(0, Samples).Select(_ => rows[random.Next(rows.Count)]).ToArray();
        var timings = new double[picks.Length];
        var next = -1;
        var workers = Enumerable.Range(0, parallelism).Select(_ => Task.Run(
            async () =>
            {
                while (Interlocked.Increment(ref next) is var i && i < picks.Length)
                {
                    var (user, session, role) = picks[i];
                    var started = Stopwatch.GetTimestamp();
                    var result = await Loader.LoadForRequestAsync(UserId.From(user), SessionId.From(session), RoleId.From(role), Ct);
                    timings[i] = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
                    result.Status.ShouldBe(AccessLoadStatus.Loaded);
                }
            },
            Ct));
        await Task.WhenAll(workers);

        Array.Sort(timings);
        return $"p50 {timings[timings.Length / 2]:F2} ms，p95 {timings[(int)(timings.Length * 0.95)]:F2} ms，p99 {timings[(int)(timings.Length * 0.99)]:F2} ms，最大 {timings[^1]:F2} ms";
    }
}
