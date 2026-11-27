namespace Kmic.BuildingBlocks.Application.Numbering;

/// <summary>声明编号规则；支持占位符 {yyyy} {yy} {MM} {dd}（日期）、{seq:N}（至少 N 位序号）和 {partition}（分组键）。</summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class NumberRuleAttribute(string format) : Attribute
{
    public string Format { get; } = format;

    /// <summary>序号重置周期，须与格式中的日期占位符匹配。</summary>
    public NumberReset Reset { get; init; }

    /// <summary>是否按分组键独立计数；启用时格式中须包含 {partition}。</summary>
    public bool Partitioned { get; init; }
}
