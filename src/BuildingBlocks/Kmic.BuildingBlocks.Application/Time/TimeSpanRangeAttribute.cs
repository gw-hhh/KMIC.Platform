using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kmic.BuildingBlocks.Application.Time;

/// <summary>
/// 配置里的时间长度必须在某个范围内
/// </summary>
/// <remarks>
/// 用它代替 <c>[Range(typeof(TimeSpan), "…", "…")]</c>：后者第一次校验时才把字符串边界转成 TimeSpan，
/// 同一个特性实例被几个宿主同时校验（集成测试里多个宿主并行启动）会撞上 .NET 里的竞态而抛 InvalidCastException。
/// 这里在构造时就解析好边界，之后只读，没有这个问题
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class TimeSpanRangeAttribute : ValidationAttribute
{
    private readonly TimeSpan _minimum;
    private readonly TimeSpan _maximum;

    /// <summary>
    /// 声明范围，两端都包含
    /// </summary>
    /// <param name="minimum">下限，如 <c>00:01:00</c> 或 <c>90.00:00:00</c></param>
    /// <param name="maximum">上限，写法同下限</param>
    public TimeSpanRangeAttribute(string minimum, string maximum)
    {
        _minimum = TimeSpan.Parse(minimum, CultureInfo.InvariantCulture);
        _maximum = TimeSpan.Parse(maximum, CultureInfo.InvariantCulture);
        if (_maximum < _minimum)
        {
            throw new ArgumentOutOfRangeException(nameof(maximum), maximum, "上限不能小于下限");
        }
    }

    /// <inheritdoc />
    public override bool IsValid(object? value) => value is null || (value is TimeSpan span && span >= _minimum && span <= _maximum);

    /// <inheritdoc />
    public override string FormatErrorMessage(string name) =>
        string.Format(CultureInfo.InvariantCulture, "{0} 必须在 {1} 到 {2} 之间", name, _minimum, _maximum);
}
