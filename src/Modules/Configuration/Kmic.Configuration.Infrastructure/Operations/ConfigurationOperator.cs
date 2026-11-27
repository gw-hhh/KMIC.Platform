using System.Text.Json;
using System.Text.Json.Nodes;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Domain.Settings;
using Kmic.Configuration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Infrastructure.Operations;

/// <summary>运维端系统设置写入工具，通过独立运维连接直接写库</summary>
public static class ConfigurationOperator
{
    /// <summary>保存系统设置（未保存过时新建记录；自定义校验失败时返回失败且不写库）</summary>
    /// <param name="connectionString">运维账号数据库连接串</param>
    /// <param name="definition">设置定义</param>
    /// <param name="value">设置值，类型须与定义一致</param>
    public static async Task<Result> SetSettingAsync(string connectionString, SystemSettingDefinition definition, object value, string reason, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var valid = definition.Validate(value);
        if (valid.IsFailure)
        {
            return valid;
        }

        var key = definition.Key;
        var valueJson = JsonSerializer.Serialize(value, definition.ValueType, KmicJsonOptions.Stored);
        await using var db = StandaloneContexts.Create<ConfigurationDbContext>(connectionString);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var row = await db.Set<SystemSettingValue>().SingleOrDefaultAsync(v => v.Key == key, ct);
        if (row is not null && SameJson(row.Value, valueJson))
        {
            // 值未变化时仍需刷新修改时间（用于过期判定），但不重复记录变更历史
            db.Entry(row).Property(nameof(SystemSettingValue.Value)).IsModified = true;
            await db.SaveForToolAsync(ct);
            await transaction.CommitAsync(ct);
            return Result.Success();
        }

        var oldValue = row?.Value;
        if (row is null)
        {
            db.Add(SystemSettingValue.Create(key, valueJson));
        }
        else
        {
            row.Set(valueJson);
        }

        db.Add(SystemSettingChange.Record(key, oldValue, valueJson, reason.Trim(), null));
        await db.SaveForToolAsync(ct);
        await transaction.CommitAsync(ct);
        return Result.Success();
    }

    // 数据库采用 jsonb 存储，读取时的空白格式可能与写入时不同，需按语义比较
    private static bool SameJson(string saved, string value) => JsonNode.DeepEquals(JsonNode.Parse(saved), JsonNode.Parse(value));
}
