using FluentValidation;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Application.Captured.List;

internal sealed class ListCapturedQueryValidator : AbstractValidator<ListCapturedQuery>
{
    public ListCapturedQueryValidator() =>
        RuleFor(q => q.Channel).Must(code => code is null || DeliveryChannel.TryFromCode(code, out _)).WithMessage("渠道只能是 Email 或 Sms");
}
