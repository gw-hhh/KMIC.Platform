using FluentValidation;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates.Revert;

internal sealed class RevertTemplateCommandValidator : AbstractValidator<RevertTemplateCommand>
{
    public RevertTemplateCommandValidator()
    {
        RuleFor(c => c.Key).NotEmpty().MaximumLength(NotificationTemplate.KeyMaxLength);
        RuleFor(c => c.Channel).NotNull();
    }
}
