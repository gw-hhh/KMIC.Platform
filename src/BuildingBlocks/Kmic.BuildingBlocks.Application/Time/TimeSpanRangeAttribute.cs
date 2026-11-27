using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kmic.BuildingBlocks.Application.Time;

/// <summary>校验配置项的时间跨度范围，替代 <c>[Range(typeof(TimeSpan), …)]</c> 以避免多宿主并发校验类型转换异常。</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class TimeSpanRangeAttribute : ValidationAttribute
{
    private readonly TimeSpan _minimum;
    private readonly TimeSpan _maximum;

    /// <summary>指定有效时间范围（包含上下限，格式如 <c>00:01:00</c> 或 <c>90.00:00:00</c>）。</summary>
    public TimeSpanRangeAttribute(string minimum, string maximum)
    {
        _minimum = TimeSpan.Parse(minimum, CultureInfo.InvariantCulture);
        _maximum = TimeSpan.Parse(maximum, CultureInfo.InvariantCulture);
        if (_maximum < _minimum)
        {
            throw new ArgumentOutOfRangeException(nameof(maximum), maximum, "上限不能小于下限");
        }
    }

    public override bool IsValid(object? value) => value is null || (value is TimeSpan span && span >= _minimum && span <= _maximum);

    public override string FormatErrorMessage(string name) =>
        string.Format(CultureInfo.InvariantCulture, "{0} 必须在 {1} 到 {2} 之间", name, _minimum, _maximum);
}
