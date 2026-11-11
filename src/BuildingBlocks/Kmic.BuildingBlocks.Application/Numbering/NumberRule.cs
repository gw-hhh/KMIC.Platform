using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Kmic.BuildingBlocks.Application.Numbering;

/// <summary>
/// 编号规则
/// </summary>
public sealed partial class NumberRule
{
    private const int MaxSequenceDigits = 18;

    private readonly IReadOnlyList<Segment> _segments;

    private NumberRule(string name, string module, string format, NumberReset reset, bool partitioned, IReadOnlyList<Segment> segments)
    {
        Name = name;
        Module = module;
        Format = format;
        Reset = reset;
        Partitioned = partitioned;
        _segments = segments;
        SequenceDigits = segments.Single(s => s.Kind == SegmentKind.Sequence).Digits;
    }

    private enum SegmentKind
    {
        Literal,
        Year4,
        Year2,
        Month,
        Day,
        Sequence,
        Partition,
    }

    /// <summary>
    /// 规则名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 模块
    /// </summary>
    public string Module { get; }

    /// <summary>
    /// 编号格式
    /// </summary>
    public string Format { get; }

    /// <summary>
    /// 重置周期
    /// </summary>
    public NumberReset Reset { get; }

    /// <summary>
    /// 是否按分组键分别计数
    /// </summary>
    public bool Partitioned { get; }

    /// <summary>
    /// 序号最少位数
    /// </summary>
    public int SequenceDigits { get; }

    /// <summary>
    /// 序号告警值
    /// </summary>
    /// <remarks>
    /// 序号到这个值时记警告，再往后编号就会变长
    /// </remarks>
    public long WarningThreshold => 9 * (long)Math.Pow(10, SequenceDigits - 1);

    /// <summary>
    /// 周期键
    /// </summary>
    /// <param name="businessDate">取号日期（业务时区）</param>
    public string PeriodKey(DateOnly businessDate) => Reset switch
    {
        NumberReset.Never => "-",
        NumberReset.Yearly => businessDate.ToString("yyyy", CultureInfo.InvariantCulture),
        NumberReset.Monthly => businessDate.ToString("yyyyMM", CultureInfo.InvariantCulture),
        NumberReset.Daily => businessDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"未知的重置周期 {Reset}。"),
    };

    /// <summary>
    /// 分组键，不分组的规则固定为 "-"
    /// </summary>
    /// <exception cref="ArgumentException">分组键缺失、格式不对，或不分组的规则传了分组键</exception>
    public string PartitionKey(string? partition)
    {
        if (!Partitioned)
        {
            return partition is null ? "-" : throw new ArgumentException($"编号规则 {Name} 不分组，不能传分组键。", nameof(partition));
        }

        return partition is not null && PartitionPattern().IsMatch(partition)
            ? partition
            : throw new ArgumentException($"编号规则 {Name} 的分组键必须是 1～10 位大写字母或数字，实际为“{partition}”。", nameof(partition));
    }

    /// <summary>
    /// 生成编号
    /// </summary>
    /// <param name="businessDate">取号日期（业务时区）</param>
    /// <param name="sequence">序号，从 1 开始</param>
    /// <param name="partition">分组键</param>
    public string Render(DateOnly businessDate, long sequence, string? partition)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sequence, 1);
        var partitionKey = PartitionKey(partition);

        var number = new StringBuilder(Format.Length + 8);
        foreach (var segment in _segments)
        {
            number.Append(segment.Kind switch
            {
                SegmentKind.Literal => segment.Text,
                SegmentKind.Year4 => businessDate.ToString("yyyy", CultureInfo.InvariantCulture),
                SegmentKind.Year2 => businessDate.ToString("yy", CultureInfo.InvariantCulture),
                SegmentKind.Month => businessDate.ToString("MM", CultureInfo.InvariantCulture),
                SegmentKind.Day => businessDate.ToString("dd", CultureInfo.InvariantCulture),
                SegmentKind.Sequence => sequence.ToString("D" + segment.Digits.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
                SegmentKind.Partition => partitionKey,
                _ => throw new InvalidOperationException($"未知的编号段 {segment.Kind}。"),
            });
        }

        return number.ToString();
    }

    /// <summary>
    /// 解析格式，有问题时记入 problems 并返回空
    /// </summary>
    internal static NumberRule? Parse(string name, string module, NumberRuleAttribute declaration, List<string> problems)
    {
        var countBefore = problems.Count;
        var segments = ParseSegments(name, declaration.Format, problems);
        if (problems.Count > countBefore)
        {
            return null;
        }

        var kinds = segments.Select(s => s.Kind).ToHashSet();
        var sequences = segments.Count(s => s.Kind == SegmentKind.Sequence);
        if (sequences != 1)
        {
            problems.Add($"{name}：格式中必须有且只有一个 {{seq:N}}，现在有 {sequences} 个。");
        }

        var partitions = segments.Count(s => s.Kind == SegmentKind.Partition);
        if (declaration.Partitioned ? partitions != 1 : partitions != 0)
        {
            problems.Add($"{name}：Partitioned = true 时格式中必须有且只有一个 {{partition}}，否则不能有。");
        }

        // 格式里没有区分周期的日期段时不能重置，否则不同周期会生成相同的编号
        var hasYear = kinds.Contains(SegmentKind.Year4) || kinds.Contains(SegmentKind.Year2);
        var periodDistinct = declaration.Reset switch
        {
            NumberReset.Yearly => hasYear,
            NumberReset.Monthly => hasYear && kinds.Contains(SegmentKind.Month),
            NumberReset.Daily => hasYear && kinds.Contains(SegmentKind.Month) && kinds.Contains(SegmentKind.Day),
            _ => true,
        };
        if (!periodDistinct)
        {
            problems.Add($"{name}：重置周期为 {declaration.Reset}，格式中缺少区分周期的日期段，不同周期会产生相同编号。");
        }

        return problems.Count > countBefore
            ? null
            : new NumberRule(name, module, declaration.Format, declaration.Reset, declaration.Partitioned, segments);
    }

    private static List<Segment> ParseSegments(string name, string format, List<string> problems)
    {
        var segments = new List<Segment>();
        var position = 0;
        foreach (Match placeholder in PlaceholderPattern().Matches(format))
        {
            AddLiteral(format[position..placeholder.Index]);
            position = placeholder.Index + placeholder.Length;

            var token = placeholder.Groups["token"].Value;
            if (ParsePlaceholder(token) is { } segment)
            {
                segments.Add(segment);
            }
            else
            {
                problems.Add($"{name}：格式中的 {{{token}}} 不是可用的占位符。");
            }
        }

        AddLiteral(format[position..]);
        return segments;

        void AddLiteral(string text)
        {
            if (text.Length == 0)
            {
                return;
            }

            if (text.AsSpan().ContainsAny('{', '}'))
            {
                problems.Add($"{name}：格式中有不成对的花括号：“{text}”。");
            }

            segments.Add(new Segment(SegmentKind.Literal, text, 0));
        }
    }

    private static Segment? ParsePlaceholder(string token) => token switch
    {
        "yyyy" => new Segment(SegmentKind.Year4, string.Empty, 0),
        "yy" => new Segment(SegmentKind.Year2, string.Empty, 0),
        "MM" => new Segment(SegmentKind.Month, string.Empty, 0),
        "dd" => new Segment(SegmentKind.Day, string.Empty, 0),
        "partition" => new Segment(SegmentKind.Partition, string.Empty, 0),
        _ when token.StartsWith("seq:", StringComparison.Ordinal)
            && int.TryParse(token.AsSpan(4), NumberStyles.None, CultureInfo.InvariantCulture, out var digits)
            && digits is >= 1 and <= MaxSequenceDigits
            => new Segment(SegmentKind.Sequence, string.Empty, digits),
        _ => null,
    };

    [GeneratedRegex(@"\{(?<token>[^{}]*)\}", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex PlaceholderPattern();

    [GeneratedRegex("^[A-Z0-9]{1,10}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex PartitionPattern();

    private sealed record Segment(SegmentKind Kind, string Text, int Digits);
}
