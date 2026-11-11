using Microsoft.CodeAnalysis;

namespace Kmic.BuildingBlocks.Analyzers.Common;

/// <summary>
/// 创建诊断描述；框架规则一律为错误级别（05-01 第 11 节、06 第 15 节）
/// </summary>
internal static class Rules
{
    public const string DomainCategory = "Kmic.Domain";
    public const string GeneralCategory = "Kmic.General";
    public const string DataCategory = "Kmic.Data";

    public static DiagnosticDescriptor Create(string id, string category, string title, string message, params string[] customTags) =>
        new(id, title, message, category, DiagnosticSeverity.Error, isEnabledByDefault: true, customTags: customTags);

    /// <summary>
    /// 集合类型中可以从外部增删元素的那些；只读接口、不可变集合、冻结集合不算
    /// </summary>
    public static bool IsMutableCollection(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol)
        {
            return true;
        }

        if (type is not INamedTypeSymbol named || IsReadOnlyCollectionType(named))
        {
            return false;
        }

        return IsCollectionInterface(named) || named.AllInterfaces.Any(IsCollectionInterface);
    }

    public static bool IsCollection(ITypeSymbol type) =>
        type.SpecialType != SpecialType.System_String
        && (type is IArrayTypeSymbol
            || type.AllInterfaces.Any(i => i.SpecialType == SpecialType.System_Collections_IEnumerable)
            || type.SpecialType == SpecialType.System_Collections_IEnumerable);

    private static bool IsReadOnlyCollectionType(INamedTypeSymbol type)
    {
        var ns = type.ContainingNamespace?.ToDisplayString();
        return string.Equals(ns, "System.Collections.Immutable", StringComparison.Ordinal)
            || string.Equals(ns, "System.Collections.Frozen", StringComparison.Ordinal)
            || type.Name.StartsWith("IReadOnly", StringComparison.Ordinal)
            || type.Name.StartsWith("ReadOnly", StringComparison.Ordinal);
    }

    private static bool IsCollectionInterface(INamedTypeSymbol type) =>
        type.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_ICollection_T
        || string.Equals(type.OriginalDefinition.ToDisplayString(), "System.Collections.ICollection", StringComparison.Ordinal);
}
