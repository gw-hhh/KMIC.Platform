using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 启动时检查容量保护的配置：数值互相矛盾时熔断会静默地错误运行，宁可拒绝启动
/// </summary>
internal sealed class MessagingOptionsValidator : IValidateOptions<MessagingOptions>
{
    private const string Prefix = MessagingOptions.Section + ":Admission:";

    public ValidateOptionsResult Validate(string? name, MessagingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var admission = options.Admission;
        var problems = new List<string>();
        if (admission.SampleInterval < TimeSpan.FromSeconds(1))
        {
            problems.Add($"{Prefix}SampleInterval 不能小于 1 秒。");
        }

        if (admission.SoftRatio is <= 0 or >= 1 || admission.HardRatio is <= 0 or > 1 || admission.SoftRatio >= admission.HardRatio)
        {
            problems.Add($"{Prefix}SoftRatio 与 HardRatio 要满足 0 < SoftRatio < HardRatio ≤ 1（现在是 {admission.SoftRatio}、{admission.HardRatio}）。");
        }

        if (admission.MinUnfinishedBytes <= 0)
        {
            problems.Add($"{Prefix}MinUnfinishedBytes 要大于 0。");
        }

        if (admission.LowWaterBytes < 0 || admission.LowWaterBytes >= admission.MinUnfinishedBytes)
        {
            problems.Add($"{Prefix}LowWaterBytes 要不小于 0 且小于 MinUnfinishedBytes，否则触发软线后永远恢复不了或一直在边缘抖动。");
        }

        return problems.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(problems);
    }
}
