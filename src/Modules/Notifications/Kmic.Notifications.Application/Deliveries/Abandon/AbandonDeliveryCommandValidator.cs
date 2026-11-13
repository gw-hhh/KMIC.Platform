using FluentValidation;

namespace Kmic.Notifications.Application.Deliveries.Abandon;

internal sealed class AbandonDeliveryCommandValidator : AbstractValidator<AbandonDeliveryCommand>
{
    public AbandonDeliveryCommandValidator() => RuleFor(c => c.Reason).NotEmpty().MaximumLength(500);
}
