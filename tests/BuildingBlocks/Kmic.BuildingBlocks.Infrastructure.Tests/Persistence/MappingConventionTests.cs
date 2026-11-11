using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 映射约定：编码列和检查约束、值对象、值对象集合、不区分大小写的列、唯一约束对应的错误
/// </summary>
public sealed class MappingConventionTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task ValueKinds_RoundTrip()
    {
        var ticket = NewTicket();
        ticket.Close();
        ticket.Escalate();
        ticket.ContactAt(EmailAddress.Create(" someone@example.com ").Value);
        ticket.Plan(Money.Of(1234.56m, "CNY"));
        ticket.Watch(Watcher.Of("乙", TicketPriority.Urgent));
        ticket.Watch(Watcher.Of("丙", TicketPriority.Normal));
        await _harness.AddAsync(_alice, ticket);

        await using var work = _harness.Begin(_alice);
        var loaded = await work.Query.Tickets.SingleAsync(t => t.Id == ticket.Id, Ct);

        loaded.Status.ShouldBe(TicketStatus.Closed);
        loaded.Priority.ShouldBe(TicketPriority.Urgent);
        loaded.ContactEmail.ShouldBe(ticket.ContactEmail);
        loaded.Budget.ShouldBe(Money.Of(1234.56m, "CNY"));
        loaded.Watchers.ShouldBe([Watcher.Of("乙", TicketPriority.Urgent), Watcher.Of("丙", TicketPriority.Normal)]);
    }

    [Fact]
    public async Task OptionalValueObject_NullRoundTrips()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());

        await using var work = _harness.Begin(_alice);
        var loaded = await work.Query.Tickets.SingleAsync(t => t.Id == ticket.Id, Ct);

        loaded.Budget.ShouldBeNull();
        loaded.ContactEmail.ShouldBeNull();
        loaded.Watchers.ShouldBeEmpty();
    }

    [Fact]
    public async Task Columns_FollowNamingAndTypeConventions()
    {
        var columns = await ColumnsAsync("Ticket");

        columns["Status"].ShouldBe("character varying(64)");
        columns["Priority"].ShouldBe("character varying(64)");
        columns["ContactEmail"].ShouldBe("character varying(256)");
        columns["BudgetAmount"].ShouldBe("numeric(18,2)");
        columns["BudgetCurrency"].ShouldBe("character varying(3)");
        columns["Watchers"].ShouldBe("jsonb");
        columns["Note"].ShouldBe("text");
    }

    [Theory]
    [InlineData("Status")]
    [InlineData("Priority")]
    public async Task CheckConstraint_RejectsUnknownCode(string column)
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());

        var error = await Should.ThrowAsync<PostgresException>(() => ExecuteAsync(
            $"""UPDATE "Probe"."Ticket" SET "{column}" = 'Bogus' WHERE "Id" = '{ticket.Id.Value}'"""));

        error.SqlState.ShouldBe(PostgresErrorCodes.CheckViolation);
        error.ConstraintName.ShouldBe($"CK_Ticket_{column}");
    }

    [Fact]
    public async Task CaseInsensitiveColumn_EqualityIgnoresCase()
    {
        var ticket = await _harness.AddAsync(_alice, Ticket.Create($"ci-{TicketNumbers.Next()}", _alice.UserId, _org["总部"]));

        await using var work = _harness.Begin(_alice);
        var upper = ticket.Number.ToUpperInvariant();

        (await work.Query.Tickets.SingleAsync(t => t.Number == upper, Ct)).Id.ShouldBe(ticket.Id);
    }

    [Fact]
    public async Task UniqueViolation_MapsToDeclaredError()
    {
        var number = $"dup-{TicketNumbers.Next()}";
        await _harness.AddAsync(_alice, Ticket.Create(number, _alice.UserId, _org["总部"]));

        // 编号不区分大小写，只差大小写也算重复
        var error = await Should.ThrowAsync<DbUpdateException>(() =>
            _harness.AddAsync(_alice, Ticket.Create(number.ToUpperInvariant(), _alice.UserId, _org["总部"])));

        var postgres = error.InnerException.ShouldBeOfType<PostgresException>();
        postgres.SqlState.ShouldBe(PostgresErrorCodes.UniqueViolation);
        UniqueConstraintErrors.Find(postgres.SchemaName, postgres.ConstraintName).ShouldBeSameAs(TicketErrors.NumberDuplicated);
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private static Ticket NewTicket() => Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]);

    private async Task ExecuteAsync(string sql)
    {
        await using var command = database.DataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<Dictionary<string, string>> ColumnsAsync(string table)
    {
        await using var command = database.DataSource.CreateCommand("""
            SELECT a.attname, format_type(a.atttypid, a.atttypmod)
            FROM pg_attribute a
            JOIN pg_class t ON t.oid = a.attrelid
            JOIN pg_namespace n ON n.oid = t.relnamespace
            WHERE n.nspname = 'Probe' AND t.relname = $1 AND a.attnum > 0 AND NOT a.attisdropped
            """);
        command.Parameters.Add(new NpgsqlParameter { Value = table });
        await using var reader = await command.ExecuteReaderAsync(Ct);
        var columns = new Dictionary<string, string>(StringComparer.Ordinal);
        while (await reader.ReadAsync(Ct))
        {
            columns[reader.GetString(0)] = reader.GetString(1);
        }

        return columns;
    }
}
