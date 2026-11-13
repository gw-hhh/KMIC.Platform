using FluentValidation;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Application.Deliveries.List;

internal sealed class ListDeliveriesQueryValidator : AbstractValidator<ListDeliveriesQuery>
{
    public ListDeliveriesQueryValidator()
    {
        RuleFor(q => q.Channel).Must(code => code is null || DeliveryChannel.TryFromCode(code, out _)).WithMessage("渠道只能是 Email 或 Sms");
        RuleFor(q => q.Status).Must(code => code is null || DeliveryStatus.TryFromCode(code, out _)).WithMessage("状态不认识");
        RuleFor(q => q.TemplateKey).MaximumLength(200);
        RuleFor(q => q).Must(q => q.From is null || q.To is null || q.From < q.To).WithMessage("结束时间要晚于开始时间");
    }
}
