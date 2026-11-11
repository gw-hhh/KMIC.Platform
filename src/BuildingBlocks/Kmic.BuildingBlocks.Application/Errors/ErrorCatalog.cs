using System.Collections.Frozen;
using System.Reflection;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Errors;

/// <summary>
/// 错误目录：全部命名为 *Errors 的静态类里声明的错误
/// </summary>
/// <remarks>
/// 前端据此生成错误码常量。两个不同的错误用了同一个错误码时拒绝启动
/// </remarks>
public sealed class ErrorCatalog
{
    private readonly FrozenDictionary<string, ErrorDefinition> _byCode;

    private ErrorCatalog(IEnumerable<ErrorDefinition> definitions) =>
        _byCode = definitions.ToFrozenDictionary(d => d.Code, StringComparer.Ordinal);

    /// <summary>
    /// 全部错误，按错误码排序
    /// </summary>
    public IReadOnlyCollection<ErrorDefinition> All => [.. _byCode.Values.OrderBy(d => d.Code, StringComparer.Ordinal)];

    /// <summary>
    /// 从程序集中读取
    /// </summary>
    public static ErrorCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()));
    }

    /// <summary>
    /// 从给定类型中读取，问题汇总后一次报出
    /// </summary>
    public static ErrorCatalog Build(IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(types);

        var errors = types
            .Where(t => t is { IsAbstract: true, IsSealed: true } && t.Name.EndsWith("Errors", StringComparison.Ordinal))
            .SelectMany(ErrorsOf)
            .ToList();

        var problems = errors
            .GroupBy(e => e.Error.Code, StringComparer.Ordinal)
            .Where(g => g.Select(e => e.Error).Distinct().Count() > 1)
            .Select(g => $"错误码 {g.Key} 在 {string.Join("、", g.Select(e => e.Owner))} 里重复声明。")
            .ToList();
        if (problems.Count > 0)
        {
            throw new DeclarationException("错误目录", problems);
        }

        return new ErrorCatalog(errors.Select(e => e.Error).DistinctBy(e => e.Code, StringComparer.Ordinal).Select(e => new ErrorDefinition(e.Code, e.Type, e.Message)));
    }

    private static IEnumerable<(Error Error, string Owner)> ErrorsOf(Type type)
    {
        const BindingFlags Flags = BindingFlags.Public | BindingFlags.Static;
        var fields = type.GetFields(Flags).Where(f => f.FieldType == typeof(Error)).Select(f => ((Error?)f.GetValue(null), $"{type.Name}.{f.Name}"));
        var properties = type.GetProperties(Flags).Where(p => p.PropertyType == typeof(Error) && p.GetIndexParameters().Length == 0).Select(p => ((Error?)p.GetValue(null), $"{type.Name}.{p.Name}"));
        return fields.Concat(properties).Where(x => x.Item1 is not null).Select(x => (x.Item1!, x.Item2));
    }
}
