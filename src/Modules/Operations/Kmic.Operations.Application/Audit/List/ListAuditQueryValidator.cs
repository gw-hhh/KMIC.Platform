using FluentValidation;

namespace Kmic.Operations.Application.Audit.List;

internal sealed class ListAuditQueryValidator : AbstractValidator<ListAuditQuery>
{
    /// <summary>
    /// 一次最多查多少天
    /// </summary>
    public const int MaxRangeDays = 93;

    public ListAuditQueryValidator()
    {
        RuleFor(q => q.From).NotEmpty().WithMessage("必须给起始时间");
        RuleFor(q => q.To).NotEmpty().WithMessage("必须给结束时间");
        RuleFor(q => q.To).GreaterThan(q => q.From).When(q => q.From != default && q.To != default).WithMessage("结束时间必须晚于起始时间");
        RuleFor(q => q.To).Must((q, to) => to - q.From <= TimeSpan.FromDays(MaxRangeDays))
            .When(q => q.From != default && q.To > q.From)
            .WithMessage($"一次最多查 {MaxRangeDays} 天");
        RuleFor(q => q.Module).MaximumLength(50);
        RuleFor(q => q.Action).MaximumLength(200);
        RuleFor(q => q.ResourceType).MaximumLength(100);
    }
}
