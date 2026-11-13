using FluentValidation;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates.Publish;

internal sealed class PublishTemplateCommandValidator : AbstractValidator<PublishTemplateCommand>
{
    public PublishTemplateCommandValidator()
    {
        RuleFor(c => c.Key).NotEmpty().MaximumLength(NotificationTemplate.KeyMaxLength);
        RuleFor(c => c.Channel).NotNull();
        RuleFor(c => c.Subject).NotEmpty().MaximumLength(TemplateVersion.SubjectMaxLength);
        RuleFor(c => c.Body).NotEmpty().MaximumLength(TemplateVersion.BodyMaxLength);
        RuleFor(c => c.Remark).MaximumLength(TemplateVersion.RemarkMaxLength);
    }
}
