using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 表注释来自配置，列注释默认来自属性的 XML 注释，框架列由约定补上
/// </summary>
public sealed class CommentTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("Ticket", "Title", "标题")]
    [InlineData("Ticket", "BudgetAmount", "预算：数额")]
    [InlineData("Ticket", "Id", "ID")]
    [InlineData("Ticket", "CreatedAt", "创建时间")]
    [InlineData("TicketLine", "TicketId", "Ticket ID")]
    public async Task Column_HasComment(string table, string column, string expected)
    {
        await using var command = database.DataSource.CreateCommand("""
            SELECT col_description(c.oid, a.attnum)
            FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace JOIN pg_attribute a ON a.attrelid = c.oid
            WHERE n.nspname = 'Probe' AND c.relname = $1 AND a.attname = $2
            """);
        command.Parameters.Add(new NpgsqlParameter { Value = table });
        command.Parameters.Add(new NpgsqlParameter { Value = column });

        (await command.ExecuteScalarAsync(Ct)).ShouldBe(expected);
    }

    [Fact]
    public async Task Table_HasComment()
    {
        await using var command = database.DataSource.CreateCommand("""SELECT obj_description('"Probe"."Ticket"'::regclass, 'pg_class')""");

        ((string?)await command.ExecuteScalarAsync(Ct)).ShouldStartWith("测试单据；Probe 模块");
    }
}
