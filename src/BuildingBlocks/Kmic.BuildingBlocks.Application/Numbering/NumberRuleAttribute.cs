namespace Kmic.BuildingBlocks.Application.Numbering;

/// <summary>
/// 编号规则
/// </summary>
/// <remarks>
/// 占位符：{yyyy} {yy} {MM} {dd} 是取号日期，{seq:N} 是至少 N 位的序号，{partition} 是分组键，其他字符原样输出
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
    /// 重置周期，要和格式里的日期段对得上
    /// </summary>
    public NumberReset Reset { get; init; }

    /// <summary>
    /// 是否按分组键分别计数，是的话格式里要有 {partition}
    /// </summary>
    public bool Partitioned { get; init; }
}
