using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 本类的单据都由同一个专用用户创建，只查这个用户的数据，其他测试类的数据不会混进来
/// </summary>
public sealed class KmicFunctionsTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncLifetime
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _owner = TestIdentity.User("匹配").InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Scope(ScopeLevel.Self));

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using var check = _harness.Begin(_owner);
        if (await check.Query.Tickets.AnyAsync(Ct))
        {
            return;
        }

        foreach (var (number, title) in new[] { ("ABC-001", "华东客户 ACME"), ("abc-002", "华南客户 acme"), ("X_Y", "折扣 50% 客户"), ("XzY", "折扣 500 客户") })
        {
            await _harness.AddAsync(_owner, Ticket.Create(number, _owner.UserId, _org["总部"], title));
        }
    }

    [Theory]
    [InlineData("acme", 2)]
    [InlineData("客户", 4)]
    [InlineData("50%", 1)]
    [InlineData("华东", 1)]
    public async Task ContainsText_DefaultColumn(string term, int expected)
    {
        await using var work = _harness.Begin(_owner);

        (await work.Query.Tickets.CountAsync(t => KmicFunctions.ContainsText(t.Title, term), Ct)).ShouldBe(expected);
    }

    [Theory]
    [InlineData("abc", 2)]
    [InlineData("x_y", 1)]
    [InlineData("_", 1)]
    public async Task ContainsText_CaseInsensitiveColumn(string term, int expected)
    {
        await using var work = _harness.Begin(_owner);

        (await work.Query.Tickets.CountAsync(t => KmicFunctions.ContainsText(t.Number, term), Ct)).ShouldBe(expected);
    }

    [Fact]
    public async Task StartsWithText_MatchesPrefixOnly()
    {
        await using var work = _harness.Begin(_owner);

        (await work.Query.Tickets.CountAsync(t => KmicFunctions.StartsWithText(t.Number, "ab"), Ct)).ShouldBe(2);
        (await work.Query.Tickets.CountAsync(t => KmicFunctions.StartsWithText(t.Number, "001"), Ct)).ShouldBe(0);
    }

    [Fact]
    public void OutsideQuery_Throws() => Should.Throw<InvalidOperationException>(() => KmicFunctions.ContainsText("a", "a"));

    public async ValueTask DisposeAsync() => await _harness.DisposeAsync();
}
