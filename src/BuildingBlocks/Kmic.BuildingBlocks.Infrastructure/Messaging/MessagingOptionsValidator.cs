using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

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
            problems.Add($"{Prefix}SoftRatio 与 HardRatio 须满足 0 < SoftRatio < HardRatio ≤ 1（当前值为 {admission.SoftRatio}、{admission.HardRatio}）。");
        }

        if (admission.MinUnfinishedBytes <= 0)
        {
            problems.Add($"{Prefix}MinUnfinishedBytes 须大于 0。");
        }

        if (admission.LowWaterBytes < 0 || admission.LowWaterBytes >= admission.MinUnfinishedBytes)
        {
            problems.Add($"{Prefix}LowWaterBytes 须大于等于 0 且小于 MinUnfinishedBytes，否则触发软线后将无法恢复或在阈值边缘频繁抖动。");
        }

        return problems.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(problems);
    }
}
