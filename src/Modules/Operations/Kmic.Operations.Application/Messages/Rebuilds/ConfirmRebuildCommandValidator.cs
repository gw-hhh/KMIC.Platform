using FluentValidation;

namespace Kmic.Operations.Application.Messages.Rebuilds;

internal sealed class ConfirmRebuildCommandValidator : AbstractValidator<ConfirmRebuildCommand>
{
    public ConfirmRebuildCommandValidator()
    {
        RuleFor(c => c.Subscription).NotEmpty().MaximumLength(200);
    }
}
