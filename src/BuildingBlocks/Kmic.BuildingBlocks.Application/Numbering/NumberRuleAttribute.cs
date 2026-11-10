namespace Kmic.BuildingBlocks.Application.Numbering;

/// <summary>
/// 标在编号规则名常量上，声明编号格式
/// </summary>
/// <remarks>
/// 占位符：{yyyy} {yy} {MM} {dd} 取业务时区下的取号日期；{seq:N} 至少 N 位的序号；{partition} 分组键；其他字符原样输出
/// </remarks>
/// <param name="format">编号格式，如 "SP-{yyyy}{MM}-{seq:4}"</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class NumberRuleAttribute(string format) : Attribute
{
    /// <summary>
    /// 编号格式
    /// </summary>
    public string Format { get; } = format;

    /// <summary>
    /// 重置周期；必须与格式中的日期段相容
    /// </summary>
    public NumberReset Reset { get; init; }

    /// <summary>
    /// 是否按分组键分别计数；为 true 时格式中必须有 {partition}
    /// </summary>
    public bool Partitioned { get; init; }
}
