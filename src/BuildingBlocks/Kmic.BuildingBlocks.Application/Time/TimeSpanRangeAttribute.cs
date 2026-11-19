using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kmic.BuildingBlocks.Application.Time;

/// <summary>
/// 配置里的时间长度必须在某个范围内
/// </summary>
/// <remarks>
/// 用它代替 <c>[Range(typeof(TimeSpan), …)]</c>，后者在多个宿主并行校验时会出错
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
