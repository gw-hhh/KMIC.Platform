using FluentValidation;

namespace Kmic.Operations.Application.Audit.List;

internal sealed class ListAuditQueryValidator : AbstractValidator<ListAuditQuery>
{
    /// <summary>
    /// 单次查询的最大天数
    /// </summary>
    public const int MaxRangeDays = 93;

    public ListAuditQueryValidator()
    {
        RuleFor(q => q.From).NotEmpty().WithMessage("请指定起始时间");
        RuleFor(q => q.To).NotEmpty().WithMessage("请指定结束时间");
        RuleFor(q => q.To).GreaterThan(q => q.From).When(q => q.From != default && q.To != default).WithMessage("结束时间必须晚于起始时间");
        RuleFor(q => q.To).Must((q, to) => to - q.From <= TimeSpan.FromDays(MaxRangeDays))
            .When(q => q.From != default && q.To > q.From)
            .WithMessage($"单次查询时间范围不能超过 {MaxRangeDays} 天");
        RuleFor(q => q.Module).MaximumLength(50);
        RuleFor(q => q.Action).MaximumLength(200);
        RuleFor(q => q.ResourceType).MaximumLength(100);
    }
}
