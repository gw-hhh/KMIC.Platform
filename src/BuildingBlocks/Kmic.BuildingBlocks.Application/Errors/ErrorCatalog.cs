using System.Collections.Frozen;
using System.Reflection;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Errors;

/// <summary>汇总各 *Errors 静态类中声明的错误定义。</summary>
public sealed class ErrorCatalog
{
    private readonly FrozenDictionary<string, ErrorDefinition> _byCode;

    private ErrorCatalog(IEnumerable<ErrorDefinition> definitions) =>
        _byCode = definitions.ToFrozenDictionary(d => d.Code, StringComparer.Ordinal);

    /// <summary>全部错误定义，按错误码排序。</summary>
    public IReadOnlyCollection<ErrorDefinition> All => [.. _byCode.Values.OrderBy(d => d.Code, StringComparer.Ordinal)];

    public static ErrorCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()));
    }

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
