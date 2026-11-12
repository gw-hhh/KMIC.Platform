using FluentValidation;

namespace Kmic.Identity.Application.SecurityEvents;

internal sealed class ListSecurityEventsQueryValidator : AbstractValidator<ListSecurityEventsQuery>
{
    public static readonly TimeSpan MaxRange = TimeSpan.FromDays(31);

    public ListSecurityEventsQueryValidator()
    {
        RuleFor(q => q.To).GreaterThan(q => q.From).WithMessage("结束时间要晚于起始时间");
        RuleFor(q => q).Must(q => q.To - q.From <= MaxRange).WithName("To").WithMessage("时间范围最长 31 天");
        RuleFor(q => q.Type).MaximumLength(100);
        RuleFor(q => q.ClientIp).MaximumLength(45);
        RuleFor(q => q.Account).MaximumLength(100);
    }
}
