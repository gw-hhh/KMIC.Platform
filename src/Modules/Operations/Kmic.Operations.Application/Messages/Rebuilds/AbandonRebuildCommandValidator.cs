using FluentValidation;

namespace Kmic.Operations.Application.Messages.Rebuilds;

internal sealed class AbandonRebuildCommandValidator : AbstractValidator<AbandonRebuildCommand>
{
    public AbandonRebuildCommandValidator()
    {
        RuleFor(c => c.Subscription).NotEmpty().MaximumLength(200);
    }
}
