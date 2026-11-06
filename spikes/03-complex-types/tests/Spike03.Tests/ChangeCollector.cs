using System.Collections;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Spike03.Tests;

// 05-09 第 3.3 节拦截器 6 的核心：只记录值变化的字段；值对象按成员展开（CreditLimit.Amount），
// 值对象集合（jsonb）只记录前后整体值；单值值对象按转换后的原始值记录
internal static class ChangeCollector
{
    private static readonly JsonSerializerOptions _json = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static List<string> Collect(EntityEntry entry)
    {
        var changes = new List<string>();
        foreach (var property in entry.Properties.Where(p => p.IsModified))
        {
            Add(changes, property.Metadata.Name, property.Metadata, property.OriginalValue, property.CurrentValue);
        }

        foreach (var complex in entry.ComplexProperties)
        {
            Collect(complex, complex.Metadata.Name, changes);
        }

        foreach (var collection in entry.ComplexCollections.Where(c => c.IsModified))
        {
            var original = entry.OriginalValues[collection.Metadata];
            // 映射到私有字段的集合（_contacts）按列名记录
            var name = collection.Metadata.ComplexType.GetContainerColumnName() ?? collection.Metadata.Name;
            changes.Add($"{name}: {Json(original)} → {Json(collection.CurrentValue)}");
        }

        return changes;
    }

    private static void Collect(ComplexPropertyEntry complex, string path, List<string> changes)
    {
        foreach (var property in complex.Properties.Where(p => p.IsModified))
        {
            Add(changes, $"{path}.{property.Metadata.Name}", property.Metadata, property.OriginalValue, property.CurrentValue);
        }

        foreach (var nested in complex.ComplexProperties)
        {
            Collect(nested, $"{path}.{nested.Metadata.Name}", changes);
        }
    }

    private static void Add(List<string> changes, string path, IProperty property, object? original, object? current)
    {
        var converter = property.GetValueConverter();
        var before = converter is null || original is null ? original : converter.ConvertToProvider(original);
        var after = converter is null || current is null ? current : converter.ConvertToProvider(current);
        if (!Equals(before, after))
        {
            changes.Add($"{path}: {Format(before)} → {Format(after)}");
        }
    }

    private static string Format(object? value) => value is null ? "null" : Convert.ToString(value, CultureInfo.InvariantCulture)!;

    private static string Json(IEnumerable? value) => value is null ? "null" : JsonSerializer.Serialize(value, _json);
}
