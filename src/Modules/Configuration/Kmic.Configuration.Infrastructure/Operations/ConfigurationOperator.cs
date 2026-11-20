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

/// <summary>
/// 运维命令的实现：写系统设置
/// </summary>
/// <remarks>
/// 直接用运维连接执行，不经过命令管道。调用方给出设置的定义和值，键、校验和保存的 JSON 形状都由定义决定，与命令管道里保存设置的形状一致。值有变化时留一条变更记录；值没变只更新修改时间
/// </remarks>
public static class ConfigurationOperator
{
    /// <summary>
    /// 保存一项系统设置，原来没有保存过就新建
    /// </summary>
    /// <param name="connectionString">运维账号的连接串</param>
    /// <param name="definition">设置的定义</param>
    /// <param name="value">值，类型要与定义的值类型一致</param>
    /// <param name="reason">原因</param>
    /// <param name="ct">取消</param>
    /// <returns>值没有通过设置自己的校验时返回校验失败，不写库</returns>
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
            // 值没变：定期写入的设置要让修改时间往前走（据此判断它有没有过期），变更记录不重复写
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

    // 库里是 jsonb，读出来的空白和写进去的不一样，要按内容比较
    private static bool SameJson(string saved, string value) => JsonNode.DeepEquals(JsonNode.Parse(saved), JsonNode.Parse(value));
}
