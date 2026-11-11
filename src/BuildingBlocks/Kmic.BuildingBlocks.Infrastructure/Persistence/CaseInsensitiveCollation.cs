using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 不区分大小写的排序规则
/// </summary>
/// <remarks>
/// 列在建表时就绑定到具体的排序规则对象。只靠 IF NOT EXISTS 的话，库里已有一个区分大小写的同名对象时会直接沿用，唯一约束就不再忽略大小写，所以创建之后还要核对
/// </remarks>
public static class CaseInsensitiveCollation
{
    private const string Locale = "und-u-ks-level2";

    /// <summary>
    /// 创建并核对 public."CaseInsensitive"
    /// </summary>
    /// <param name="connection">迁移账号的连接</param>
    /// <param name="ct">取消令牌</param>
    /// <exception cref="InvalidOperationException">定义不对，或别的 schema 里有同名对象</exception>
    public static async Task EnsureAsync(NpgsqlConnection connection, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(connection);

        await using (var create = connection.CreateCommand())
        {
            create.CommandText = $"""
                CREATE COLLATION IF NOT EXISTS public."{Collations.CaseInsensitive}" (provider = icu, locale = '{Locale}', deterministic = false)
                """;
            await create.ExecuteNonQueryAsync(ct);
        }

        await using var check = connection.CreateCommand();
        check.CommandText = """
            SELECT n.nspname, c.collprovider::text, c.colllocale, c.collisdeterministic
            FROM pg_collation c JOIN pg_namespace n ON n.oid = c.collnamespace
            WHERE c.collname = $1
            """;
        check.Parameters.Add(new NpgsqlParameter { Value = Collations.CaseInsensitive });

        var problems = new List<string>();
        await using var reader = await check.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var schema = reader.GetString(0);
            if (!string.Equals(schema, "public", StringComparison.Ordinal))
            {
                problems.Add($"schema {schema} 中有同名排序规则 \"{Collations.CaseInsensitive}\"，建表时可能被解析到它，删除后重试。");
            }
            else if (!string.Equals(reader.GetString(1), "i", StringComparison.Ordinal)
                || !string.Equals(await reader.IsDBNullAsync(2, ct) ? null : reader.GetString(2), Locale, StringComparison.Ordinal)
                || reader.GetBoolean(3))
            {
                problems.Add($"public.\"{Collations.CaseInsensitive}\" 已存在但定义不符，应为 provider = icu、locale = '{Locale}'、deterministic = false。");
            }
        }

        if (problems.Count > 0)
        {
            throw new InvalidOperationException("不区分大小写的排序规则有误：" + string.Join(" ", problems));
        }
    }

    /// <summary>
    /// 核对不区分大小写的列，确认都绑定到 public."CaseInsensitive"
    /// </summary>
    /// <param name="connection">数据库连接</param>
    /// <param name="context">模块的写入上下文</param>
    /// <param name="ct">取消令牌</param>
    /// <exception cref="InvalidOperationException">有列绑定到了别的排序规则</exception>
    public static async Task VerifyColumnsAsync(NpgsqlConnection connection, DbContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(context);

        var problems = new List<string>();
        foreach (var (schema, table, column) in CaseInsensitiveColumns(context.GetService<IDesignTimeModel>().Model))
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT cn.nspname || '.' || c.collname
                FROM pg_attribute a
                JOIN pg_class t ON t.oid = a.attrelid
                JOIN pg_namespace tn ON tn.oid = t.relnamespace
                LEFT JOIN pg_collation c ON c.oid = a.attcollation
                LEFT JOIN pg_namespace cn ON cn.oid = c.collnamespace
                WHERE tn.nspname = $1 AND t.relname = $2 AND a.attname = $3
                """;
            command.Parameters.Add(new NpgsqlParameter { Value = schema });
            command.Parameters.Add(new NpgsqlParameter { Value = table });
            command.Parameters.Add(new NpgsqlParameter { Value = column });

            var bound = await command.ExecuteScalarAsync(ct) as string;
            if (!string.Equals(bound, $"public.{Collations.CaseInsensitive}", StringComparison.Ordinal))
            {
                problems.Add($"\"{schema}\".\"{table}\".\"{column}\" 绑定的是 {bound ?? "（无）"}。");
            }
        }

        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"不区分大小写的列没有绑定到 public.\"{Collations.CaseInsensitive}\"：" + string.Join(" ", problems));
        }
    }

    private static IEnumerable<(string Schema, string Table, string Column)> CaseInsensitiveColumns(IModel model) =>
        from entityType in model.GetEntityTypes()
        let table = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table)
        where table is not null
        from property in entityType.GetFlattenedProperties()
        where string.Equals(property.GetCollation(), Collations.CaseInsensitive, StringComparison.Ordinal)
        select (table.Value.Schema ?? "public", table.Value.Name, property.GetColumnName(table.Value)!);
}
